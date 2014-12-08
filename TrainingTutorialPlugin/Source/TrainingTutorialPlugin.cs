/*!
  \file SimpleTutorialPlugin.cs

  \brief Demonstration tutorial plugin for Kerbal Space Program.

  \author ilo <ilo@symracing.net>
  \version 1.0

  Copyright (C) 2012 Felipe Falanghe
  
  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
  \source http://forum.kerbalspaceprogram.com/content/121-Writing-Tutorials-A-Demo-%28and-some-source-code%29

*/
using UnityEngine;

namespace ilo
{
    /*!
     This project is meant to be a "Hello World" example showing how to make a Tutorial plugin.

     The main class for your plug-in should implement TutorialScenario, which is a Kerbal Space 
     Program class that handles the basic systems for the tutorial, like managing the instructor
     dialog and the page flow, etc.
     https://github.com/Anatid/XML-Documentation-for-the-KSP-API/blob/master/src/TutorialScenario.cs

     For a tutorial plugin, the KSPAddon attribute is not required, as the base class already 
     implements the required persistance for the tutorial management. A tutorial plugin instance
     is meant to be created everytime the scenario is loaded.

     The tutorial flow is managed by a state machine, which is based off the same one that the 
     Kerbal EVA controller uses. It is based around the concept of States and Events. States hold 
     the code that gets run when that state is active, and Events are used to move from one state 
     to another.

     The base TutorialScenario has a built-in tutorial FSM called Tutorial. In the OnTutorialSetup 
     method, you create the tutorial pages and the events that will change them, add it all into 
     the Tutorial solver, and start it.    
  
     This demonstration plugin is a copy of TutorialDemo, a class already included in the game.
    */

    // Tutorials extend TutorialScenario (which in turn extends ScenarioModule).
    public class TrainingTutorialPlugin : TutorialScenario
    {

        /*!
         The First thing to define are the tutorial steps. Every step involves a TutorialPage object
         and (probably) a KFSMEvents. These two components are the basis of any tutorial system.
         
         the TutorialPages are the different states in the TutorialScenario state machine.
        */
        TutorialPage defaultPage1, defaultPage2, defaultPage3, specialPage1;

        /*!
         Events can be used along with tutorial pages, to manage situations where the player 
         strays from the plan. Events come in two forms: KFSMEvents and KFSMTimedEvents. KFSMEvents 
         are similar to states (and pages) in some ways. They also have callbacks that get called 
         randomly from different code parts or at specific times. They also have a GoToStateOnEvent 
         value, which holds a reference to the state you want to transition to after the event is 
         triggered.
         
         Events are defined independently so when you add them, you can assign them to any 
         combination of states. If you've ever seen an FSM diagram, the analogy becomes simple. 
         States are the nodes, and Events are the arrows that connect each node.
         \todo include graphical reference.
         
        */ 
        KFSMEvent onSomethingUnplanned, onTutorialRestart;
        KFSMTimedEvent onStayTooLongOnPage1;


        /*!
          OnAssetSetup method is called on Start, before anything else can run, to allow the plugin
          to define the assets used by the tutorial. In this demonstration plugin, the instructor 
          name is changed to "Gene", instead of using the default "Wernher" kerbal.
         
          This is the place where the plugin must load or grab references to other assets that might
          me necessary.
         */
        protected override void OnAssetSetup()
        {
            instructorPrefabName = "Instructor_Gene";
        }


        /*!
          OnTutorialSetup is where the whole tutorial must be defined. This is the place to define
          and create the different training steps, pages, conditions or events.
         */
        protected override void OnTutorialSetup()
        {
            // start up a default demonstration tutorial. Create a simple dialog window using the 
            // default skin for tutorial scenarios. This is just a cosmetic adjustment on the 
            // training layout.
            defaultPage1 = new TutorialPage("default page 1");
            defaultPage1.windowTitle = "Tutorial Window";


            /*!
              TutorialPages are states in the tutorial state machine. They have a number of 
              callback that get called as the tutorial progresses, in which you can add your 
              own code. 
              
              This demo uses a coding style known as lambda expressions to assign logic to each 
              callback , without having to write methods somewhere else in the code. This is just 
              to keep it all in one place, but you can do it the conventional way if you prefer.

              Note that you don't really need to assign a method to every one of TutorialPage 
              callbacks. They all default to an empty method, so it's safe to omit the ones you 
              don't need. The TutorialPage callback definition is:

              TutorialPage.OnEnter(KFSMState st) - gets called once when the page becomes the 
              active one. The 'st' parameter is a reference to the last state before this one.

              TutorialPage.OnUpdate() - gets called repeatedly (from Update), to let you run 
              your update logic for the state.

              TutorialPage.OnFixedUpdate() - gets called repeatedly (from FixedUpdate), to let
              you run your fixed update logic for the state.

              TutorialPage.OnLateUpdate() - gets called repeatedly (from LateUpdate), to let you
              run your late update logic for the state.

              TutorialPage.OnDrawContent() - gets called repeatedly (from OnGUI) to let you draw
              your GUI content using Unity's GUI classes.

              TutorialPage.OnLeave(KFSMState st) - gets called once when the tutorial is about
              to move to a new page. The 'st' parameter is a reference to that next state.

              TutorialPage.GoToNextPage() - call this to make the tutorial go to the next state.
              Pages are sequenced by the order in which they get added.

              TutorialPage.GoToPrevPage() - same as above, only goes back to the previous page.

              TutorialPage.SetAdvanceCondition(KFSMEventConditio n c) - Use this to set a condition
              which will be evaluated repeatedly to advance to the next step when the condition is 
              met. This is just a convenience method to reduce repeated code. It's the same as 
              checking for the same condition on one of the state update events and manually 
              calling GoToNextPage.             
             */

            /*!
              We are at the begining of the Training tutorial, but this can be cause because of
              a user reset. Make sure the Kerbal is not doing any fancy animation and just sits
              waiting for the next event.
             */
            defaultPage1.OnEnter = (KFSMState st) =>
            {
                instructor.StopRepeatingEmote();
            };



            /*!
              The desmonstration tutorial plugins will show a welcome message, a next button and
              a countdown to 0.
             
              Create the instructor dialog content for this tutorial step. In this case we 
              include a counter on the description label that will show a countdown from 10 to 0. 
              This demonstration plugin includes a timed event that gets fired after 10 seconds 
              while the player is not avanced to the next page. 
            */
            defaultPage1.OnDrawContent = () =>
            {
                // Add this step description
                GUILayout.Label("This is my demo tutorial to test out the tutorial scenario features." +
                                " Press Next to go to the next page, or wait " +
                                (10 - Tutorial.TimeAtCurrentState).ToString("0") + " seconds.", GUILayout.ExpandHeight(true));

                // Add the corresponding training driving buttons.
                if (GUILayout.Button("Next")) Tutorial.GoToNextPage();
            };

            /*!
              When the page setup is finished, it has to be added to the Tutorial handler.
             */ 
            Tutorial.AddPage(defaultPage1);



            /*!
              Add the second training page. This page is actually showcasing the PREV / NEXT
              functionality as a result of the internal FSM inherited by the base class.
             
              This page also enables a Kerbal animation.
             */
            defaultPage2 = new TutorialPage("default page 2");
            defaultPage2.windowTitle = "Tutorial Window (continued)";
            defaultPage2.OnEnter = (KFSMState st) =>
            {
                instructor.PlayEmoteRepeating(instructor.anim_idle_lookAround, 5f);
            };

            /*!
              Update this tutorial step description by creating the dialog window.
             */
            defaultPage2.OnDrawContent = () =>
            {
                GUILayout.Label("This second page is only here to test the state progression system." +
                                " Tutorial pages can be stepped forward, and also stepped back.", GUILayout.ExpandHeight(true));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Back")) Tutorial.GoToLastPage();
                if (GUILayout.Button("Next")) Tutorial.GoToNextPage();
                GUILayout.EndHorizontal();
            };
            Tutorial.AddPage(defaultPage2);



            /*!
              This training tutorial ends here. This 3rd page will show an end message. The main 
              difference with the previos tutorial page is that this one includes a "Reset" button.
             */
            defaultPage3 = new TutorialPage("default page 3");
            defaultPage3.windowTitle = "Tutorial Window (last one)";
            defaultPage3.OnEnter = (KFSMState st) =>
            {
                instructor.PlayEmoteRepeating(instructor.anim_true_nodA, 5f);
            };



            /*!
              Restarting the tutorial (by hitting the 'Reset' button or programatically, the 
              tutorial plugin will recreate the initial scenario conditions. 
             */
            defaultPage3.OnDrawContent = () =>
            {
                GUILayout.Label("This third page is also only here to test the state progression system." +
                                " It's very much like the previous one, but it has a button to restart the tutorial.", GUILayout.ExpandHeight(true));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Back")) Tutorial.GoToLastPage();
                if (GUILayout.Button("Restart")) Tutorial.RunEvent(onTutorialRestart);
                GUILayout.EndHorizontal();
            };
            Tutorial.AddPage(defaultPage3);



            /*!
              On this Tutorial demonstration, there is a TimedEvent assigned to run on page 1 of 
              the tutorial. It's set to go off after ten seconds, and take you to a special page. 
              You'll notice that this special page isn't a TutorialPage object, but a KFSMState. 
              That's fine, since TutorialPage is just an extension of KFSMState. Adding a state 
              that isn't a page to the tutorial is perfectly possible. 
              
              Also notice that the special page is added by calling Tutorial.AddState instead of 
              AddPage. This lets the tutorial know that this page isn't part of the standard 
              sequence, so it doesn't get in the way of the normal flow when calling GoToNextPage,
              for instance.
            */
            specialPage1 = new TutorialPage("special page 1");
            specialPage1.OnEnter = (KFSMState lastSt) =>
            {
                specialPage1.windowTitle = "Tutorial Window (from " + lastSt.name + ")";
                specialPage1.onAdvanceConditionMet.GoToStateOnEvent = lastSt;

                // Make the instructor kerbal drop a smile and show recongnition of its stupidity.
                instructor.PlayEmote(instructor.anim_true_thumbsUp);
            };
            specialPage1.OnDrawContent = () =>
            {
                GUILayout.Label("This Page shows that it's possible to use external events to send the tutorial to" +
                                " any arbitrary state, even ones not in the default sequence. Use this to handle cases where the" +
                                " player strays off the plan.\n\nNote that this page is added with AddState instead of AddPage," +
                                " because we don't want this page to be part of the normal tutorial sequence.", GUILayout.ExpandHeight(true));

                if (GUILayout.Button("Yep"))
                {
                    Tutorial.RunEvent(specialPage1.onAdvanceConditionMet);
                }
            };
            specialPage1.OnLeave = (KFSMState st) =>
            {
                instructor.PlayEmote(instructor.anim_idle_sigh);
            };
            Tutorial.AddState(specialPage1);



            /*!
             Besides the regular events existing in the tutorial FSM system, it is possible
             to add other events as required. In this demonstration tutorial, some additional
             events have to be created to complete all the possible use cases.
             */

            /*!
              The last page of the tutorial includes the possibility to restart the whole 
              training tutorial. This is not a regular FSM event and as such, we have to include
              to the tutorial event list.
             
              The restart operation is a manually triggered event, associated to a button in 
              the last tutorial page.
             */
            onTutorialRestart = new KFSMEvent("Tutorial Restarted");
            onTutorialRestart.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
            onTutorialRestart.GoToStateOnEvent = defaultPage1;
            Tutorial.AddEvent(onTutorialRestart, defaultPage3);


            /*!
              At some point of a training tutorial, you might need to verify that a certain
              achievement is get in a reasonable ammount of time. This task can be performed
              using a timed event. 
             
              This even will get executed after 10 seconds and is associated to the page 1 of
              this demonstration tutorial. If the user does not click the next button during
              this period of time, the tutorial will jump automatically to the SpecialPage1
              special state page.
             */ 
            onStayTooLongOnPage1 = new KFSMTimedEvent("Too Long at Page 1", 10.0);
            onStayTooLongOnPage1.GoToStateOnEvent = specialPage1;
            Tutorial.AddEvent(onStayTooLongOnPage1, defaultPage1);



            /*!
              When a timeout event in the first tutorial page occurs, a special state page is 
              shown describing what has happened. This special page is not part of the tutorial
              page flow and requires its custom event to go back to a known state. This is the 
              purpose of this event.
             */
            onSomethingUnplanned = new KFSMEvent("Something Unplanned");
            onSomethingUnplanned.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
            onSomethingUnplanned.GoToStateOnEvent = specialPage1;
            Tutorial.AddEventExcluding(onSomethingUnplanned, specialPage1);



            /*!
              At this point the tutorial is already defined, all pages configured and events
              created: It is time to run the Training course!
             */
            Tutorial.StartTutorial(defaultPage1);
        }



        /*! 
          For convenience, lets expose a method from this Tutorial so any external component
          can inform us about any particular event to react appropriatelly.
           
          This method would be called by some external component or the TutorialPlugin itself.
         */
        public void SomethingUnplanned()
        {
            if (Tutorial.Started)
            {
                Tutorial.RunEvent(onSomethingUnplanned);
            }
        }

    }
}