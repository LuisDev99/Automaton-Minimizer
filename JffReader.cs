using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace Computer_Theory___Minimize_an_Automaton
{

    class JffReader
    {
        public string file_path { get; set; }
        public List<StateNode> state_nodes;
        public List<string> alphabet;

        public JffReader(string _file_path)
        {
            this.file_path = _file_path;
            this.state_nodes = new List<StateNode>();
            this.alphabet = new List<string>();
            Read_jff_file();
        }

        public string Get_State_Name_By(int id)
        {
            foreach(StateNode statenode in state_nodes)
            {
                if (statenode.id == id)
                    return statenode.name;
            }

            return string.Empty;
        }

        public void Search_and_Add_Node_Transition(int from, int to, string read)
        {
            foreach(StateNode statenode in state_nodes)
            {
                if(statenode.id == from)
                {
                    statenode.AddNodeTransition(to, read, Get_State_Name_By(to));
                    return;
                }
            }

        }

        public void Read_jff_file()
        {

            XmlDocument xml_doc = new XmlDocument();
            xml_doc.Load(file_path);

            foreach(XmlNode automaton in xml_doc.DocumentElement)
            {

                if(automaton.Name == "automaton")
                {
                    foreach(XmlNode state_or_transition in automaton.ChildNodes)
                    {
                        if (state_or_transition.Name == "state")
                        {
                            int id = Convert.ToInt32(state_or_transition.Attributes[0].InnerText);
                            string name = state_or_transition.Attributes[1].InnerText;
                            double x = 0, y = 0;
                            bool isInitial = false, isFinal = false;

                            foreach (XmlNode location in state_or_transition.ChildNodes)
                            {
                                if(location.Name == "x")
                                {
                                    x = Convert.ToDouble(location.FirstChild.Value); //First Convert to Double because in file, this value is saved as double, and then to int
                                }

                                else if(location.Name == "y")
                                {
                                    y = Convert.ToDouble(location.FirstChild.Value); //First Convert to Double because in file, this value is saved as double, and then to int

                                    //At this point, the state has been read, its x and y, name and id
                                    //state_nodes.Add(new StateNode(id, x, y, name));

                                    //Break this foreach because the rest of the nodes are just garbage text
                                }
                                else if(location.Name == "initial")
                                {
                                    isInitial = true;
                                }
                                else if(location.Name == "final")
                                {
                                    isFinal = true;
                                }
                            }

                            state_nodes.Add(new StateNode(id, x, y, name, isInitial, isFinal));

                        } else if(state_or_transition.Name == "transition")
                        {
                            int from = 0, to = 0;
                            string read = "";

                            foreach(XmlNode transition in state_or_transition.ChildNodes)
                            {
                                if(transition.Name == "from")
                                {
                                    from = Convert.ToInt32(transition.FirstChild.Value);
                                } else if(transition.Name == "to")
                                {
                                    to = Convert.ToInt32(transition.FirstChild.Value);
                                } else if (transition.Name == "read")
                                {
                                    //When this point is reached, we have read a transition, soo
                                    //we have to find the node with the id that matches "from", and set that node
                                    //its transition's properties
                                    read = transition.FirstChild.Value;

                                    Search_and_Add_Node_Transition(from, to, read);

                                    if (!this.alphabet.Any(e => e == read))
                                        this.alphabet.Add(read);
                               
                                    break;
                                }
                            }
                        }
                    }
                }             
            }
        }

        public string Minimize_Automaton()
        {

            /*
             *  Vertical States      | stair array
             *       2              -> []
             *       3              -> [] []
             *       4              -> [] [] []
             *       5              -> [] [] [] []
             *  Horizontal States ---> 1  2  3  4  
             */ 

            StateNode[] vertical_states, horizontal_states;
            sbyte[][] stair_array;

            /* Check if its DFA */
            if (!Check_IF_DFA())
            {
                return "The loaded automaton isnt a DFA";
            }

            /* First, find unreacheable states and delete them */
            Delete_Unreacheable_States();

            /*Initialize*/
            vertical_states = new StateNode[state_nodes.Count - 1];
            horizontal_states = new StateNode[state_nodes.Count - 1];
            stair_array = new sbyte[state_nodes.Count - 1][];

            /* Then, build every array */
            Build_2D_Stair_Array(ref stair_array);
            Build_Vertical_State_Array(ref vertical_states);
            Build_Horizontal_State_Array(ref horizontal_states);

            /* Then, mark with 0 those states that are distintive from one another */
            bool is_any_state_distinct = Find_Distinct_State(vertical_states, horizontal_states, ref stair_array);

            /* If no state was distinct from one or another, then just stop the algorithm */
            if (!is_any_state_distinct)
                return "The automaton cant be minimized! All states are the same (final or not final)";

            /* Keep doing a while loop till no new distinct states appear */
            bool did_new_state_appear = false;
            sbyte counter = 1;
           
            do
            {
                did_new_state_appear = Mark_Distinct_State(vertical_states, horizontal_states, ref stair_array, counter);
                counter++;
                
            } while (did_new_state_appear);

            /* Check if the automaton was minimized already to send a message that says that the automaton is already minimized */

            if (Automaton_Is_Already_Minimized(stair_array))
            {
                return "The loaded Automaton is minimized already!";
            }

   
            /* Now, create merge new states into a single one and also convert the 2D array to list array */
            state_nodes = Convert_To_List(vertical_states, horizontal_states, stair_array); //TODO: This, convert all the empty spaces from the stair array into a single statenode, merging all of its transitions

            return "Success! The loaded automaton is minimized! Try saving it now";
        }

        private bool Check_IF_DFA()
        {
            /*First, check in every transition if a character didnt repeat*/
            foreach(StateNode state in state_nodes)
            {
                for(int i = 0; i  < state.transitions.Count; i++)
                {
                    for(int j = i + 1; j < state.transitions.Count; j++)
                    {
                        if (state.transitions[i].read == state.transitions[j].read)
                            return false;
                    }
                }
            }

            /*Now, check if every state's transition has a length equal to the alphabet size */
            foreach(StateNode state in state_nodes)
            {
                if (state.transitions.Count != this.alphabet.Count)
                    return false;
            }

            return true;
        }

        private bool Automaton_Is_Already_Minimized(sbyte[][] stair_array)
        {
            for(int i = 0; i < state_nodes.Count - 1; i++)
            {
                for(int j = 0; j < stair_array[i].Length; j++)
                {
                    //if at least one position is empty (-1) in the stair array, it means that the automaton can be minimized
                    if (stair_array[i][j] == -1)
                        return false;
                }
            }

            return true;
        }

        private bool Mark_Distinct_State(StateNode[] vArray, StateNode[] hArray, ref sbyte[][] stair_array, sbyte counter)
        {
            bool was_a_state_distinct = false;

            for (int i = 0; i < state_nodes.Count - 1; i++) // i for vArray
            {
                for (int j = 0; j < stair_array[i].Length; j++) // J for hArray
                {
                    int previous_count = counter - 1;
                    if(RelationShip_Exist_Between_State(vArray[i], hArray[j], stair_array, Convert.ToSByte(previous_count), vArray, hArray))
                    {
                        if(stair_array[i][j] == -1) //Only mark if that spot was empty (-1)
                            stair_array[i][j] = counter;

                        was_a_state_distinct = true;
                    }
                }
            }

            return was_a_state_distinct;
        }

        private bool RelationShip_Exist_Between_State(StateNode vState, StateNode hState, sbyte[][] stair_array, sbyte counter, StateNode[] vArray, StateNode[] hArray)
        {
            /*
             * (1,a) = 2        (1,b) = 6
             * (2,a) = 7        (2,b) = 3
             * ---------------------------
             * (2,7) = no       (6,3) = ?
             * (7,2) = ?        (3,6) = si
             */

            sbyte nextCount = Convert.ToSByte(counter + 1);

            //For every letter of the alphabet, check the states relationships

            for(int k = 0; k < this.alphabet.Count; k++)
            {
                #region Primera Tupla Con Letra
                //Get both state's destination with the first letter of the alphabet
                int firstPairID_1 = Get_Destination_From_State(vState, this.alphabet[k]); //(first_state, a) = 2, returns -1 if this state didnt have a destination
                int firstPairID_2 = Get_Destination_From_State(hState, this.alphabet[k]); //(second_state, a) = 7

                //Find this ID its real index from the vertical and horizontal position
                for (int i = 0; i < state_nodes.Count - 1; i++)
                {
                    if (vArray[i].id == firstPairID_1)
                    {
                        firstPairID_1 = i;
                        break;
                    }
                }

                //X2
                for (int i = 0; i < state_nodes.Count - 1; i++)
                {
                    //Missing by one index! Estoy buscando por id y por nombre y por eso tira indeces diferentes
                    if (hArray[i].id == firstPairID_2)
                    {
                        firstPairID_2 = i;
                        break;
                    }
                }

                // (7,2) Check the stair array if this tuple has a mark available with the current counter in that position
                if (firstPairID_1 != -1 && firstPairID_2 != -1)
                {
                    // (7,2)
                    if (firstPairID_2 < stair_array[firstPairID_1].Length)
                    {
                        if (stair_array[firstPairID_1][firstPairID_2] == counter)
                        {
                            return true;
                        }
                    }

                }

                #endregion

                #region Primera Tupla Reverse Con Letra
                //Get both state's destination with the first letter of the alphabet
                int reversePairID_1 = Get_Destination_From_State(vState, this.alphabet[k]); //(first_state, a) = 2, returns -1 if this state didnt have a destination
                int reversePairID_2 = Get_Destination_From_State(hState, this.alphabet[k]); //(second_state, a) = 7

                //Find this ID its real index from the vertical and horizontal position
                for (int i = 0; i < state_nodes.Count - 1; i++)
                {
                    if (hArray[i].id == reversePairID_1)
                    {
                        reversePairID_1 = i;
                        break;
                    }
                }

                //X2
                for (int i = 0; i < state_nodes.Count - 1; i++)
                {
                    //Missing by one index! Estoy buscando por id y por nombre y por eso tira indeces diferentes
                    if (vArray[i].id == reversePairID_2)
                    {
                        reversePairID_2 = i;
                        break;
                    }
                }

                // (2, 7) Check the stair array if this tuple has a mark available with the current counter in that position
                if (reversePairID_1 != -1 && reversePairID_2 != -1)
                {
                    // (2, 7)
                    if (reversePairID_1 < stair_array[reversePairID_2].Length)
                    {
                        if (stair_array[reversePairID_2][reversePairID_1] == counter)
                        {
                            return true;
                        }
                    }

                }

                #endregion

            }

            return false;
        }

        private int Get_Destination_From_State(StateNode state, string read)
        {
            foreach(Transition transition in state.transitions)
            {
                if (transition.read == read)
                    return transition.to; //Return the state's id that this state is pointing to, hence, its destination
            }

            return -1;
        }

        private void Build_Horizontal_State_Array(ref StateNode[] horizontal_states)
        {
            /* To create the horizontal states, we need to add all the states except the last one*/
            for (int i = 0; i < state_nodes.Count - 1; i++)
            {
                horizontal_states[i] = state_nodes[i];
            }
        }

        private void Build_Vertical_State_Array(ref StateNode[] vertical_states)
        {
            /* To create the vertical states, we need to add all the states except the first one*/
            for (int i = 0; i < state_nodes.Count - 1; i++)
            {
                vertical_states[i] = state_nodes[i + 1];
            }
        }

        private void Build_2D_Stair_Array(ref sbyte[][] stair_array)
        {
            for (int i = 0; i < state_nodes.Count - 1; i++)
            {
                stair_array[i] = new sbyte[i + 1];
            }

            /* Initialize all the indexes values to -1 to represent a null value */
            for(int i = 0; i < state_nodes.Count -1; i++)
            {
                for(int j = 0; j < stair_array[i].Length; j++)
                {
                    stair_array[i][j] = -1;
                }
            }
        }

        /// <summary>
        ///     Function that will find in the 2D array, any state that can be merge into a single one
        ///     And then converts the stair 2D array into a list of StateNode objects
        /// </summary>
        /// <returns></returns>
        private List<StateNode> Convert_To_List(StateNode[] vArray, StateNode[] hArray, sbyte[][] stair_array)
        {
            List<StateNode> new_state_nodes = new List<StateNode>();
            List<int> ids_used = new List<int>();

            int id_counter = 0, y_counts = 0;
            double x = 70, y = 50; 

            /* Find in the stair array an empty spot and merged the two states */
            for (int i = 0; i < state_nodes.Count - 1; i++)
            {
                for (int j = 0; j < stair_array[i].Length; j++)
                {
                    //If this position in the array is empty, it means it can be merged
                    if(stair_array[i][j] == -1)
                    {
                        StateNode nodeA = vArray[i];
                        StateNode nodeB = hArray[j];

                        //Update both statenode's merge info
                        int indexOFNodeA = state_nodes.IndexOf(nodeA);
                        int indexOFNodeB = state_nodes.IndexOf(nodeB);

                        state_nodes[indexOFNodeA].mergeInfo = new MergeInfo(nodeB.name, nodeA.name, true);
                        state_nodes[indexOFNodeB].mergeInfo = new MergeInfo(nodeA.name, nodeB.name, true);



                        //Add both state's id soo that they are no longer used
                        ids_used.Add(nodeA.id);
                        ids_used.Add(nodeB.id);

                        StateNode new_state_node = new StateNode();

                        new_state_node.name = nodeB.name + ", " + nodeA.name;
                        new_state_node.id = id_counter++;
                        new_state_node.isInitial = nodeA.isInitial || nodeB.isInitial;
                        new_state_node.isFinal= nodeA.isFinal || nodeB.isFinal;
                        new_state_node.coordinate = new Coordinate(x, y);

                        //The new state node should remember its transitions
                        new_state_node.transitions = new List<Transition>(nodeA.transitions);
                        new_state_node.transitions.AddRange(nodeB.transitions);

                        new_state_nodes.Add(new_state_node);

                        x += 100;
                        y_counts++;

                        if(y_counts == 2)
                        {
                            y_counts = 0;
                            y += 100;
                        }

                    }
                }
            }

            /* Add all the old unrepeated states that in the new state nodes list */
            foreach(StateNode oldStateNode in state_nodes)
            {
                bool isThisNodeNew = true;

                foreach(int id_used in ids_used)
                {
                    if(oldStateNode.id == id_used)
                    {
                        isThisNodeNew = false;
                        break;
                    }
                }

                if (isThisNodeNew)
                {
                    oldStateNode.id = id_counter++;
                    new_state_nodes.Add(oldStateNode);
                }
            }


            /* Update transition in every state */

            for(int i = 0; i < new_state_nodes.Count; i++)
            {
                for(int j = 0; j < new_state_nodes[i].transitions.Count; j++)
                {
                    Transition t;

                    foreach (StateNode lookingForDestinationState in new_state_nodes)
                    {
                        if (lookingForDestinationState.name == new_state_nodes[i].transitions[j].destination_name || lookingForDestinationState.name.Contains(new_state_nodes[i].transitions[j].destination_name))
                        {
                            t = new Transition(lookingForDestinationState.id, new_state_nodes[i].transitions[j].read, lookingForDestinationState.name);
                            new_state_nodes[i].transitions[j] = t;
                        }
                    }
                }
            }

            /* Eliminate Duplicates */
             //List<T> withDupes = LoadSomeData();
             //List<T> noDupes = withDupes.Distinct().ToList();

            for (int i = 0; i < new_state_nodes.Count; i++)
            {
                for (int j = 0; j < new_state_nodes[i].transitions.Count; j++)
                {
                    new_state_nodes[i].transitions = new_state_nodes[i].transitions.Distinct().ToList();
                }
            }


                    //foreach(StateNode newState in new_state_nodes)
                    //{
                    //    foreach(Transition transition in newState.transitions)
                    //    {
                    //        Transition t;

                    //        foreach(StateNode lookingForDestinationState in new_state_nodes)
                    //        {
                    //            if(lookingForDestinationState.name == transition.destination_name || lookingForDestinationState.name.Contains(transition.destination_name))
                    //            {
                    //                t = new Transition(lookingForDestinationState.id, transition.read, lookingForDestinationState.name);
                    //                transition.Value = t;
                    //            }
                    //        }
                    //    }
                    //}

                    //foreach(StateNode stateNode in state_nodes)
                    //{
                    //    if (stateNode.mergeInfo.isMerge)
                    //    {
                    //        //Find the new state with the info of the old state to add the transitions
                    //        string merge_name = stateNode.mergeInfo.name_before_merge + ", " + stateNode.mergeInfo.merge_with;

                    //        foreach(StateNode newStateNode in new_state_nodes)
                    //        {
                    //           if(newStateNode.name == merge_name)
                    //           {
                    //                newStateNode.transitions = stateNode.transitions;

                    //                //check first if the old statenode transition is also merge, if it is, we have to point to the merged state
                    //                foreach(Transition transition in stateNode.transitions)
                    //                {
                    //                    string destination_name = transition.destination_name;

                    //                    foreach(StateNode findNewStateName in new_state_nodes)
                    //                    {
                    //                        if (findNewStateName.name.Contains(destination_name))
                    //                        {
                    //                            //This means that the original state transition destination is merged, so we need to point to it

                    //                        }
                    //                    }

                    //                }

                    //           }
                    //        }
                    //    }
                    //}

                    return new_state_nodes;
        }

        /// <summary>
        ///     First function that gets call when starting the minimize algorithm
        ///     Compares two nodes and marks inside the array if they are initial and final states
        ///     If all the states were the same, this function will return false to tell that this automaton cant be minimized
        /// </summary>
        /// <param name="stair_array">Gets by reference the stair array</param>
        /// <returns></returns>
        private bool Find_Distinct_State(StateNode[] vArray, StateNode[] hArray, ref sbyte[][] stair_array)
        {
            bool was_a_state_distinct = false;

            for(int i = 0; i < state_nodes.Count - 1; i++) // V for vArray
            {
                for(int j = 0; j < stair_array[i].Length; j++) // J for hArray
                {
                    if (vArray[i].isFinal != hArray[j].isFinal) //If they are distinct
                    {
                        stair_array[i][j] = 0; //TODO: Change this to zero
                        was_a_state_distinct = true;            
                    }
                }
            }

            return was_a_state_distinct;
        }

        /// <summary>
        ///     Searches throughout all the nodes and checks if its reacheable
        ///     If one node isnt being pointed by another node, this node will be
        ///     deleted from the list
        /// </summary>
        private void Delete_Unreacheable_States()
        {
            bool[] has_someone_pointing = new bool[state_nodes.Count];

            foreach(StateNode node in state_nodes)
            {
                foreach(Transition transition in node.transitions)
                {
                    //Set to true the node this transition is pointing to
                    has_someone_pointing[transition.to] = true;
                }
            }

            /* Delete the node that no one is pointing to */
            for(int i = 0; i < has_someone_pointing.Length; i++)
            {
                if(has_someone_pointing[i] == false)
                {
                    /* Search the node with the index that matches the id to delete it, cuz that index says this node isnt being pointed at */
                    foreach(StateNode node in state_nodes)
                    {
                        if(node.id == i && !node.isInitial)
                        {
                            state_nodes.Remove(node);
                            break;
                        }
                    }
                }
            }

        }

        public void Save_jff_file(string file)
        {
            //file = "C:\\Users\\DELL\\Documents\\Faze_testyi.jff";
   
            XmlTextWriter writer = new XmlTextWriter(file, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;

            /*
             * Jff XML Structure
             * <structure>
             *      <type>fa</type>
             *      <automaton>
             *          <state id = "0" name = "q0">
             *              <x>17</x>
             *              <y>20</y>
             *          </state>      
             *          <transition>
             *              <from>6</from>
             *              <to>1</to>
             *              <read>a</read>
             *          </transition>
             *      </automaton>
             * </structure>
             */

            writer.WriteStartDocument();
            writer.WriteStartElement("structure");
            writer.WriteElementString("type", "fa"); //<type>fa</type>
            writer.WriteStartElement("automaton");

            foreach(StateNode node in this.state_nodes)
            {
                writer.WriteStartElement("state");
                writer.WriteAttributeString("id", node.id.ToString());
                writer.WriteAttributeString("name", node.name);
                writer.WriteElementString("x", node.coordinate.x.ToString());
                writer.WriteElementString("y", node.coordinate.y.ToString());


                if (node.isInitial)
                {
                    writer.WriteStartElement("initial");
                    writer.WriteEndElement();
                }
                if (node.isFinal)
                {
                    writer.WriteStartElement("final");
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); // </state>
            }


            foreach (StateNode node in this.state_nodes)
            {
                foreach(Transition transition in node.transitions)
                {
                    writer.WriteStartElement("transition");
                    writer.WriteElementString("from", node.id.ToString());
                    writer.WriteElementString("to", transition.to.ToString());
                    writer.WriteElementString("read", transition.read);
                    writer.WriteEndElement(); // </transition>
                }         
            }

            writer.WriteEndElement(); // </automaton>
            writer.WriteEndElement(); // </structure>

            writer.Close();

        }
    }   
}

//Useful Garbage

//Employee[] employees = new Employee[4];
//employees[0] = new Employee(1, "David", "Smith", 10000);
//employees[1] = new Employee(3, "Mark", "Drinkwater", 30000);
//employees[2] = new Employee(4, "Norah", "Miller", 20000);
//employees[3] = new Employee(12, "Cecil", "Walker", 120000);


//writer.WriteStartDocument();
//writer.WriteStartElement("Employees");

//foreach (Employee employee in employees)
//{
//    writer.WriteStartElement("Employee");
//    writer.WriteAttributeString("Employee", "Fuck this one");
//    writer.WriteElementString("ID", employee.Id.ToString());
//    writer.WriteElementString("FirstName", employee.FirstName);
//    writer.WriteElementString("LastName", employee.LastName);
//    writer.WriteElementString("Salary", employee.Salary.ToString());

//    writer.WriteEndElement();

//}

//writer.WriteEndElement();
//writer.WriteEndDocument();

//writer.Close();

//class Employee
//{
//    int _id;
//    string _firstName;
//    string _lastName;
//    int _salary;

//    public Employee(int id, string firstName, string lastName, int salary)
//    {
//        this._id = id;
//        this._firstName = firstName;
//        this._lastName = lastName;
//        this._salary = salary;
//    }

//    public int Id { get { return _id; } }
//    public string FirstName { get { return _firstName; } }
//    public string LastName { get { return _lastName; } }
//    public int Salary { get { return _salary; } }
//}

