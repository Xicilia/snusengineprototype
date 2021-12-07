using SnusEngine;
using System;
using System.Collections.Generic;


namespace SnusGame {

    namespace Stuff {

        abstract class Item {
            public string name;

            public string about; 
            public Dictionary<string,string> info;

            public Item(string name, string about){
                this.name = name;
                this.about = about;
            }
        }

        class Snus: Item {

            public Snus(string name,string about, int heal): base(name,about){
                info = new Dictionary<string, string>() {
                    {"Heal", heal.ToString()}
                };
            }
        }
    }

    class SnusSession: SnusEngine.Session {
        public Entities.Player Player {get;set;}
        public Entities.Enemy CurrentEnemy {get;set;}

        public List<string> names {get;set;}
        public List<string> fams {get;set;}

        public List<Stuff.Snus> snuses {get;set;}

        public State PreviousState;
        public State MainState {get;set;}

        public SnusSession() : base() {}
        public SnusSession(State StartState) : base(StartState) {}
    }

    namespace Entities {

        class Bag {
            public List<Stuff.Item> items;

            public Bag() {
                items = new List<Stuff.Item>();
            }
            public Bag(List<Stuff.Item> items) {
                this.items = items;
            }
            
            public void AddToBag(Stuff.Item item) {
                items.Add(item);
            }
            public void DeleteFromBag(int index) {
                items.RemoveAt(index);
            }
            public List<Stuff.Item> getAllItems() {
                return items;
            }
        }
        static class NameParser {
            public static List<string> Parse(string filename) {
                List<string> result = new List<string>() {};
                string[] lines = System.IO.File.ReadAllLines(filename);
                foreach(string line in lines){
                    result.Add(line);
                }
                return result;
            }
        }
        static class Fightulator {

            public static bool isCritical(double CritChance){
                Random randomizer = new Random();
                return  randomizer.Next(101) <= (100 *  CritChance);
            }
            public static int CalculateDamage(Entity Attacker, Entity Deffencer){
                Random randomizer = new Random();
                int RawDamage = randomizer.Next(Attacker.MinAttack,Attacker.MaxAttack);
                return Convert.ToInt32(RawDamage * (100.0 / (100.0 + Deffencer.Deffence)));
                //return Convert.ToInt32(( 2 * Math.Pow(RawDamage,2) ) / ( RawDamage + Deffencer.Deffence ));
            }
        }
        abstract class Entity {
            
            public Bag bag {get;set;}
            public int MaxHp {get;set;}
            public int CurrentHp {get;set;}
            public int MaxAttack {get;set;}
            public int MinAttack {get;set;}
            public int Deffence {get;set;}

            public string Name {get;set;}

            public double CritChance {get;set;}
            public Entity(int Hp, int MaxAttack, int MinAttack, int Deffence){
                bag = new Bag();

                MaxHp = CurrentHp = Hp;

                Name = "Standart Entity";

                if(MaxAttack >= MinAttack){
                    this.MaxAttack = MaxAttack;
                    this.MinAttack = MinAttack;
                } else {
                    this.MaxAttack = MinAttack;
                    this.MinAttack = MaxAttack;
                }

                this.Deffence = Deffence;
                this.CritChance = CritChance;
            }
        }
        class Player : Entity {
            public Player(int Hp, int MaxAttack, int MinAttack, int Deffence) : base(Hp,MaxAttack,MinAttack,Deffence) {
            }
        }
        class Enemy : Entity {

            List<int> chances {get;set;}
            public Enemy(int Hp, int MaxAttack, int MinAttack, int Deffence) : base(Hp,MaxAttack,MinAttack,Deffence) {
                chances = new List<int>();
            }

            public void addItemToDrop(Stuff.Item item, int dropChance) {
                bag.AddToBag(item);
                chances.Add(dropChance);
            }
            public Stuff.Item getLoot(){
                System.Random randomizer = new Random();
                int randomValue = randomizer.Next(101);
                for(int i = 0; i < bag.items.Count; i++){
                    if(randomValue < chances[i]) {
                        return bag.items[i];
                    }  
                }
                return null;            
            }
        }
    }

    namespace AttackEvents {

        class StartCombatEventArgs : SnusEngine.SnusEvents.SnusEventArgs {
            public SnusSession CurrentSession;

            public StartCombatEventArgs(ref SnusSession CurrentSession) {
                this.CurrentSession = CurrentSession;
            }
        }

        class StartCombatEvent : SnusEngine.SnusEvents.SnusEvent {
            new public StartCombatEventArgs args {get;set;}

            public StartCombatEvent(ref SnusSession CurrentSession) {
                args = new StartCombatEventArgs(ref CurrentSession);
            }

            override public void EventToDo() {
                //args.CurrentSession.Player = new Entities.Player(250,25,20,5);
                Random randomizer = new Random();

                int EnemyHp = randomizer.Next(100,215);
                int EnemyMinAttack = randomizer.Next(10,25);
                int EnemyMaxAttack = randomizer.Next(4,12) + EnemyMinAttack;
                int EnemyDeffence = randomizer.Next(2,8); 

                string name = $"{args.CurrentSession.names[randomizer.Next(args.CurrentSession.names.Count - 1)]} {args.CurrentSession.fams[randomizer.Next(args.CurrentSession.fams.Count - 1)]}";

                args.CurrentSession.CurrentEnemy = new Entities.Enemy(EnemyHp,EnemyMinAttack,EnemyMaxAttack,EnemyDeffence);
                //args.CurrentSession.CurrentEnemy.bag.AddToBag(args.CurrentSession.snuses[0]);
                //args.CurrentSession.CurrentEnemy.bag.AddToBag(args.CurrentSession.snuses[1]);
                args.CurrentSession.CurrentEnemy.addItemToDrop(args.CurrentSession.snuses[2],25);
                args.CurrentSession.CurrentEnemy.addItemToDrop(args.CurrentSession.snuses[1],50);
                args.CurrentSession.CurrentEnemy.addItemToDrop(args.CurrentSession.snuses[0],90);
                args.CurrentSession.CurrentEnemy.Name = name;
                args.CurrentSession.PreviousState = args.CurrentSession.CurrentState;
                args.CurrentSession.CurrentState = new CombatState($"Fight with {args.CurrentSession.CurrentEnemy.Name}",
                $"your HP is {args.CurrentSession.Player.CurrentHp}\n{args.CurrentSession.CurrentEnemy.Name} HP is {args.CurrentSession.CurrentEnemy.CurrentHp}",
                "",ref args.CurrentSession);
            }
        }

        class EventAttackArgs : SnusEngine.SnusEvents.SnusEventArgs {
            public SnusSession CurrentSession;

            public EventAttackArgs (ref SnusSession CurrentSession) {
                this.CurrentSession = CurrentSession;
            }
        }
        class EventAttack : SnusEngine.SnusEvents.SnusEvent {
            new public EventAttackArgs args {get; set;}
            public EventAttack(ref SnusSession CurrentSession) {
                args = new EventAttackArgs(ref CurrentSession);
            }
            override public void EventToDo(){

                int ToDefenderDamage = Entities.Fightulator.CalculateDamage(args.CurrentSession.Player,args.CurrentSession.CurrentEnemy);
                int ToAttackerDamage = Entities.Fightulator.CalculateDamage(args.CurrentSession.CurrentEnemy,args.CurrentSession.Player);
                args.CurrentSession.Player.CurrentHp -= ToAttackerDamage;
                System.Threading.Thread.Sleep(750);
                args.CurrentSession.CurrentEnemy.CurrentHp -= ToDefenderDamage;
                
                if(args.CurrentSession.CurrentEnemy.CurrentHp <= 0 ) {
                  Random randomizer = new Random();
                  int stat = randomizer.Next(5);
                  string BuffPrefix = "";
                  switch(stat) {
                      case 0:
                        BuffPrefix = "hp";
                        args.CurrentSession.Player.MaxHp += 5;
                        break;
                      case 1:
                        BuffPrefix = "deffence";
                        args.CurrentSession.Player.Deffence += 1;
                        break;
                      case 2:
                        BuffPrefix = "min attack";
                        args.CurrentSession.Player.MinAttack += 2;
                        break;
                      case 3:
                        BuffPrefix = "max attack";
                        args.CurrentSession.Player.MaxAttack += 2;
                        break;
                      case 4:
                        BuffPrefix = "whole attack range";
                        args.CurrentSession.Player.MaxAttack += 1;
                        args.CurrentSession.Player.MinAttack += 1;
                        break;
                      
                
                  }

                  Stuff.Item receivedItem = args.CurrentSession.CurrentEnemy.getLoot();
                  //Stuff.Item receivedItem = args.CurrentSession.snuses[0];
                  if(receivedItem != null) args.CurrentSession.Player.bag.AddToBag(receivedItem);

                  args.CurrentSession.CurrentState = args.CurrentSession.MainState;
                  args.CurrentSession.CurrentState.BeforeDescription = $"you win this one! Your HP was restored~ \nyour {BuffPrefix} was upgraded!";
                  if(receivedItem != null){
                      args.CurrentSession.CurrentState.BeforeDescription += $"\nyou earned {receivedItem.name}!";
                  }
                  args.CurrentSession.Player.CurrentHp = args.CurrentSession.Player.MaxHp;
                  return;
                } else if(args.CurrentSession.Player.CurrentHp <= 0) {
                  args.CurrentSession.CurrentState = args.CurrentSession.MainState;
                  args.CurrentSession.CurrentState.BeforeDescription = "you lost :(";
                  return;
                }
                

                args.CurrentSession.CurrentState.BeforeDescription = $"<color=yellow>you</color> damaged <color=red>{ToDefenderDamage}</color> to <color=blue>{args.CurrentSession.CurrentEnemy.Name}\n{args.CurrentSession.CurrentEnemy.Name}</color> damaged <color=red>{ToAttackerDamage}</color> to <color=yellow>you</color>";
                args.CurrentSession.CurrentState.AfterAll = "";
                args.CurrentSession.CurrentState.Description = $"<color=yellow>your</color> HP is <color=green>{args.CurrentSession.Player.CurrentHp}</color>\n<color=blue>{args.CurrentSession.CurrentEnemy.Name}</color> HP is <color=green>{args.CurrentSession.CurrentEnemy.CurrentHp}</color>";

            }
        }
    
    }

    class ToSnusStateEventArgs: SnusEngine.SnusEvents.SnusEventArgs {
        public SnusSession CurrentSession;
        public State NewState;

        public ToSnusStateEventArgs(ref SnusSession session, State NewState) {
            CurrentSession = session;
            this.NewState = NewState;
        }
    }

    class ToSnusStateEvent: SnusEngine.SnusEvents.SnusEvent {
        new ToSnusStateEventArgs args {get;set;}

        public ToSnusStateEvent(ref SnusSession session, State NewState) {
            args = new ToSnusStateEventArgs(ref session,NewState);
        }
        override public void EventToDo() {
            args.CurrentSession.PreviousState = args.CurrentSession.CurrentState;
            args.CurrentSession.CurrentState = args.NewState;
        }
    }

    class InfoAboutEntityEventArgs: SnusEngine.SnusEvents.SnusEventArgs {
        public SnusSession CurrentSession;
        public Entities.Entity EntityToShow;

        public InfoAboutEntityEventArgs(ref SnusSession CurrentSession, Entities.Entity EntityToShow){
            this.CurrentSession = CurrentSession;
            this.EntityToShow = EntityToShow;
        }
    }
    class InfoAboutEntityEvent: SnusEngine.SnusEvents.SnusEvent {
        new InfoAboutEntityEventArgs args {get;set;}

        public InfoAboutEntityEvent(ref SnusSession CurrentSession, Entities.Entity EntityToShow) {
            args = new InfoAboutEntityEventArgs(ref CurrentSession, EntityToShow);
        }

        override public void EventToDo() {
         args.CurrentSession.PreviousState = args.CurrentSession.CurrentState;
         args.CurrentSession.CurrentState = new State(
            $"HP: {args.EntityToShow.CurrentHp}/{args.EntityToShow.MaxHp}\nAttack Range: {args.EntityToShow.MinAttack}-{args.EntityToShow.MaxAttack}\nDeffence: {args.EntityToShow.Deffence}",
            "", 
            $"INFO ABOUT: {args.EntityToShow.Name}",
             new List<Option>() {
                 new Option(new ShowBagEvent(ref args.CurrentSession,args.EntityToShow),"show bag"),
                 new Option(new ToSnusStateEvent(ref args.CurrentSession,args.CurrentSession.PreviousState),"back")
             }
         );   
        }

    }

    class ShowBagEvent: SnusEngine.SnusEvents.SnusEvent {
        SnusSession session;
        Entities.Entity entity;

        public ShowBagEvent(ref SnusSession session, Entities.Entity entity) {
            this.session = session;
            this.entity = entity;
        }
        override public void EventToDo() {
            this.session.PreviousState = this.session.CurrentState;

            List<Option> options = new List<Option>();
            /*List<Stuff.Item> items;
            try{
                items = entity.bag.getAllItems();
            } catch {
                items = new List<Stuff.Item>();
            }*/
            List<Stuff.Item> items = entity.bag.getAllItems();

            foreach(Stuff.Item item in items) {
                options.Add(
                    new Option(new InfoAboutItemEvent(ref session,item),item.name)
                );
            }
            options.Add(
                new Option(new ToSnusStateEvent(ref session,session.PreviousState),"back")
            );

            this.session.CurrentState = new State($"{entity.Name} bag",$"bag weight: {entity.bag.items.Count}","",options);
        }
    }
    class InfoAboutItemEvent: SnusEngine.SnusEvents.SnusEvent {

        SnusSession session;
        Stuff.Item item;

        public InfoAboutItemEvent(ref SnusSession CurrentSession, Stuff.Item itemToShow){
            session = CurrentSession;
            item = itemToShow;
        }
        public override void EventToDo() {
            session.PreviousState = session.CurrentState;
            
            string info = "";
            foreach(KeyValuePair<string,string> param in item.info){
                info += $"{param.Key}: {param.Value}\n";
            }
            info += item.about;

            session.CurrentState = new State(info,
            "",$"Info about {item.name}",new List<Option>() {
                 new Option(new ToSnusStateEvent(ref session,session.PreviousState),"back")
             });
        }
    }

    class CombatState : State {

        static List<Option> CombatOptions = new List<Option>() {
            new Option("<color=red>Attack</color>"),
            new Option("<color=cyan>Info About this zabivan</color>"),
            new Option("<color=white>Run away</color>")
            //new Option("Show SnusBag (later)")
        };

        public SnusSession CurrentSession;

        public CombatState(string BeforeDescription, string Description, string AfterAll, ref SnusGame.SnusSession CurrentSession): 
        base(Description,AfterAll,BeforeDescription,CombatOptions) {
            this.CurrentSession = CurrentSession;
            this.CurrentSession.CurrentState = this;
            this.CurrentSession.MainState.BeforeDescription = "";
            this.CurrentSession.MainState.Description = "Welcome to SNUSLAND";
            //this.CurrentSession.CurrentState.Description = "AAAA";
            this.Options[0].bindEvent(
                new AttackEvents.EventAttack(ref CurrentSession)
            );
            this.Options[1].bindEvent(
                new InfoAboutEntityEvent(ref CurrentSession,CurrentSession.CurrentEnemy)
            );
            this.Options[2].bindEvent(
                new ToSnusStateEvent(ref CurrentSession, new State(
                    "Are you sure?","","",new List<Option>() {
                        new Option(new ToSnusStateEvent(ref CurrentSession,CurrentSession.MainState),"yesss(da)"),
                        new Option(new ToSnusStateEvent(ref CurrentSession,this),"nooo!!!!!")
                    }
                ))
            );
        }
    }

}