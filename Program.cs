using System;
using System.Collections.Generic;
using SnusGame;

namespace SnusLand {
    class Program
    {
        static void Main(string[] args) {

            SnusSession GameSession = new SnusSession();

            string Description = "<color=red>Welcome to SNUSLAND</color>";
            //SnusEngine.ConsoleThings.WriteWithTags(Description);
            string BeforeDescription = "<color=yellow>this is only a test version</color>";
            string AfterAll = ""; 

            Console.WriteLine("Enter your name: ");
            string Name;
            try {
                Name = Console.ReadLine();
            } catch {
                Description = "You just had to enter a normal name, but you decided to enter...this? you will be DURAK";
                Name = "DURAK";
            }
            if(Name.Length >= 25) {
                Name = "DEBIL";
            }

            List<string> Names;
            List<string> Fams;
            try{
                Names = SnusGame.Entities.NameParser.Parse("data/names/names.txt");
            } catch{
                Names = new List<string>() {"ZABVIVAN ENTITY NAME"};
            }
            try{
                Fams = SnusGame.Entities.NameParser.Parse("data/names/fams.txt");
            } catch {
                Fams = new List<string>() {"ZABIVAN ENTITY FAM"};
            }

            GameSession.fams = Fams;
            GameSession.names = Names;

            SnusGame.Entities.Player CurrentPlayer = new SnusGame.Entities.Player(250,30,20,6);
            CurrentPlayer.Name = Name;
            GameSession.Player = CurrentPlayer;

            List<SnusGame.Stuff.Snus> snuses = new List<SnusGame.Stuff.Snus>() {
                new SnusGame.Stuff.Snus("<color=white>Common Snus</color>","Just consume it!",20),
                new SnusGame.Stuff.Snus("<color=green>Uncommon Snus</color>","it tastes better if you listen to rap",45),
                new SnusGame.Stuff.Snus("<color=blue>Rare Snus</color>","even ancient people consumed this snus",85),
                new SnusGame.Stuff.Snus("<color=yellow>Epic Snus</color>","only best zabivans can consume this one",125),
                new SnusGame.Stuff.Snus("<color=red>Legendary Snus</color>","according to legends, this snus was found in the grave of an unnamed king and no one has yet been able to consume it",165),
                new SnusGame.Stuff.Snus("<color=darkred>M</color><color=blue>y</color><color=green>t</color><color=magenta>h</color><color=cyan>i</color><color=red>c</color> Snus","was created by [DATA EXPUNGED]",228)
            };

            GameSession.snuses = snuses;

            List<SnusEngine.Option> SnusesOptions = new List<SnusEngine.Option>();
            foreach(SnusGame.Stuff.Snus snus in snuses){
                SnusesOptions.Add(
                    new SnusEngine.Option(new SnusGame.InfoAboutItemEvent(ref GameSession,snus),snus.name)
                );
            }

            SnusEngine.State SnusInfoState = new SnusEngine.State("<color=green>ALL SNUSES INFO</color>","","",SnusesOptions);

            List<SnusEngine.Option> Options = new List<SnusEngine.Option> {
                new SnusEngine.Option(new SnusGame.AttackEvents.StartCombatEvent(ref GameSession),"<color=green>find zabivan</color>"),
                new SnusEngine.Option(new InfoAboutEntityEvent(ref GameSession,GameSession.Player),"<color=blue>info about me</color>"),
                new SnusEngine.Option(new SnusGame.ToSnusStateEvent(ref GameSession,SnusInfoState),"SNUSES LIST")
            };
            GameSession.Player.bag.AddToBag(snuses[0]);
            
            SnusEngine.State StartState = new SnusEngine.State(Description,AfterAll,BeforeDescription,Options);
            GameSession.CurrentState = StartState;
            GameSession.MainState = StartState;
            GameSession.idle();
        }
    }
}
