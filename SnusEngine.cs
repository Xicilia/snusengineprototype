using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace SnusEngine {

    namespace SnusTags {
        static class ColorTags {

            static Dictionary<string,ConsoleColor> colors = new Dictionary<string, ConsoleColor>() {
                {"red",ConsoleColor.Red},
                {"blue",ConsoleColor.Blue},
                {"green",ConsoleColor.Green},
                {"yellow",ConsoleColor.Yellow},
                {"cyan",ConsoleColor.Cyan},
                {"white",ConsoleColor.White},
                {"darkred",ConsoleColor.DarkRed},
                {"magenta",ConsoleColor.Magenta}
            };
            public static void SetColorByTag(string tag) {
                foreach(KeyValuePair<string,ConsoleColor> color in colors) {
                    if(color.Key == tag) {
                        Console.ForegroundColor = color.Value;
                        return;
                    }
                }
            }
        }
    }

    namespace SnusEvents {
        public delegate void SnusEventDelegate(); // stuff for event

        public abstract class SnusEventArgs: EventArgs {} // abstract class for event args

        public abstract class SnusEvent { // abstract class for events
            public event SnusEventDelegate CurrentEvent; // event
            public SnusEventArgs args {get; set;} // event args

            public SnusEvent() {
                CurrentEvent += this.EventToDo; // bind method to event
            }

            public abstract void EventToDo(); // contains event code

            public void DoEvent() {
                CurrentEvent.Invoke(); // event cant be null because every option has default ZeroEvent 
            }


        }

        class ChangeStateEventArgs : SnusEventArgs {
            public State NewState; // State to change
            public Session CurrentSession;

            public ChangeStateEventArgs(State NewState, ref Session CurrentSession) {
                this.NewState = NewState;
                this.CurrentSession = CurrentSession;
            }

        }

        class ChangeStateEvent : SnusEvent  {
            
            new public ChangeStateEventArgs args {get; set;}
            public ChangeStateEvent(State NewState, ref Session CurrentSession): base() {
                args = new ChangeStateEventArgs(NewState, ref CurrentSession);
            }
            override public void EventToDo(){
                args.CurrentSession.CurrentState = args.NewState;
            }
        }  
    

        class ZeroEventArgs : SnusEventArgs {
            public Session CurrentSession;

            public ZeroEventArgs(ref Session CurrentSession) {
                this.CurrentSession = CurrentSession;
            }
        }
        class ZeroEvent : SnusEvent {

            new public ZeroEventArgs args {get; set;}

            public ZeroEvent(ref Session CurrentSession) {
                args = new ZeroEventArgs(ref CurrentSession);
            }
            override public void EventToDo(){
                args.CurrentSession.CurrentState.BeforeDescription = "<color=darkred>ZeroEvent was called</color>";
            }
        }
    }
    static class ConsoleThings {
        public static void ClearCurrentLine() {
            int PosY =  Console.GetCursorPosition().Top;
            Console.SetCursorPosition(0,PosY - 1);
            Console.Write("\r");
            for(int i = 0;i < Console.WindowWidth;i++) Console.Write(" ");
            Console.Write("\r");
            Console.SetCursorPosition(0,PosY - 1);
        }
        public static void WriteWithTags(string str) {
            int IgnoreCount = 0;
            bool IsDelayAll = false;
            int DelayTime = 0;

            for(int i = 0; i < str.Length; i++){

                if(str[i] == '<') {
                    int TagEnd = str.IndexOf('>',i);

                    string Tag = str.Substring(i + 1,TagEnd - 1 - i );

                    if(Tag.Contains("color=")) {
                        SnusTags.ColorTags.SetColorByTag(Tag.Remove(0,6));
                    } else if(Tag.Contains("/color")) {
                        Console.ForegroundColor = ConsoleColor.Gray;
                    } else if(Tag.Contains("delay=")) {
                        DelayTime = Int16.Parse(Tag.Remove(0,6));
                    } else if(Tag.Contains("delayall=")) {
                        DelayTime = Int16.Parse(Tag.Remove(0,9));
                        IsDelayAll = true;
                    }
                    IgnoreCount = TagEnd - i + 1;
                }
                if(IgnoreCount != 0){
                    IgnoreCount--;
                    continue;
                }
                if(DelayTime != 0) {
                    System.Threading.Thread.Sleep(DelayTime);
                    if(!IsDelayAll) DelayTime = 0;

                }
                Console.Write(str[i]);
            }
            Console.Write("\n");
        }
    
    }

    class State {
        public string Description {get; set;} // Second State line, cant be null
        public string AfterAll {get; set;} // last State line, can be null
        public string BeforeDescription {get; set;} // first State line, cant be null
        public List<Option> Options {get; set;} // state body, cant be null
        public int StateAnswer { // number of selected option
            get;
            private set;
        }
        public State(string Description, string AfterAll, string BeforeDescription, List<Option> Options) {
            this.Description = Description;
            this.AfterAll = AfterAll;
            this.BeforeDescription = BeforeDescription;
            this.Options = Options;
        }
    
        public void show(){
            Console.Clear();

            //DRAWING

            //HEAD
            if(BeforeDescription != "") ConsoleThings.WriteWithTags(BeforeDescription);
            //Console.WriteLine(Description);
            ConsoleThings.WriteWithTags(Description);

            //BODY
            for(int i = 0;i < Options.Count; i++){
                string OptionText = $"{i + 1}. {Options[i].Text}";
                //Console.WriteLine($"{i + 1}. {Options[i].Text}");
                ConsoleThings.WriteWithTags(OptionText);
            }
            
            //TAIL
            if(AfterAll != "") ConsoleThings.WriteWithTags(AfterAll);


        }

        public void GetInput() {
            string ReceivedAnswer;
            bool Correct = false;
            while(!Correct){ // while user input is invalid
                ReceivedAnswer = Console.ReadLine();

                if(ReceivedAnswer == ".EXIT") Environment.Exit(0);

                try {
                    StateAnswer = Int16.Parse(ReceivedAnswer);
                    
                    //less than one and more than number of options
                    if(StateAnswer < 1 || StateAnswer > Options.Count) {
                        SnusEngine.ConsoleThings.ClearCurrentLine();
                        continue;
                    }
                    break;
                    
                } catch { // if there are string in input
                    SnusEngine.ConsoleThings.ClearCurrentLine();
                    continue;
                }
            }
        }
        public void handle(){
            Options[StateAnswer - 1].DoEvent();
        }
    }

    class Session {

        public State CurrentState {get; set;} //state which currently showing

        void ShowCurrentState() {
            CurrentState.show();
            CurrentState.GetInput();
        }

        public void idle(){
            if(CurrentState == null) { // if session was started but with no state
                Console.WriteLine("Start State werent loaded");
                Console.ReadLine();
                return;
            }
            while (true) {
                ShowCurrentState();
                CurrentState.handle();
            }
        }

        public Session(State StartState){
            CurrentState = StartState;
        }
        public Session(){}

    }

    class Option {
        public string Text; // displaying text
        SnusEvents.SnusEvent OptionEvent; 
        bool Initialized;
        public void bindEvent(SnusEvents.SnusEvent NewEvent){
            //if(!Initialized) return;
            OptionEvent = NewEvent; 
        }

        public void DoEvent() {
            //if(!Initialized) return;
            OptionEvent.DoEvent();
        }

        public void setSession(ref Session BindingSession) {
            //if(Initialized) return;
            this.OptionEvent = new SnusEvents.ZeroEvent(ref BindingSession);
            Initialized = true;
        }

        public Option(SnusEvents.SnusEvent snusEvent,string Text){
            this.Text = Text;
            this.OptionEvent = snusEvent;
            Initialized = true;
        }
        public Option(ref Session CurrentSession, string Text){
            this.Text = Text;
            
            //avoid null call exception by creating zero event
            this.OptionEvent = new SnusEvents.ZeroEvent(ref CurrentSession);
            Initialized = true;
        }
        public Option(string Text){
            this.Text = Text;
            Initialized = false;
        }
    }

}