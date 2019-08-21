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
        public JffReader(string _file_path)
        {
            this.file_path = _file_path;
            this.state_nodes = new List<StateNode>();
            Read_jff_file();
        }

        public void Search_and_Add_Node_Transition(int from, int to, string read)
        {
            foreach(StateNode statenode in state_nodes)
            {
                if(statenode.id == from)
                {
                    statenode.AddNodeTransition(to, read);
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
                                    break;
                                }
                            }
                        }
                    }
                }             
            }
        }

        public void Minimize_Automaton()
        {
            byte[,] arraee = new byte[1, 1];
            //arraee[0, 1] = 2;

            /* First, find an unreacheable state */
            Delete_Unreacheable_States();

            /* Then, create the 2D stair array */
            bool is_any_state_distinct = Distinct_State();

            /* If no state was distinct from one or another, then just stop the algorithm */
            if (!is_any_state_distinct)
                return;

            /* Keep doing a while loop till no new distinct states appear */
            bool did_new_state_appear = false;
            do
            {
                did_new_state_appear = Mark_Distinct_State();
                
            } while (did_new_state_appear);


            /* Now, create merge new states into a single one and also convert the 2D array to list array */
            state_nodes = Convert_To_List();

        }

        /// <summary>
        ///     Function that will find in the 2D array, any state that can be merge into a single one
        ///     And then converts the stair 2D array into a list of StateNode objects
        /// </summary>
        /// <returns></returns>
        private List<StateNode> Convert_To_List()
        {
            return new List<StateNode>();
        }

        private bool Mark_Distinct_State()
        {
            return true;
        }

        private bool Distinct_State()
        {
            return true;
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
                    foreach(StateNode node in state_nodes)
                    {
                        if(node.id == i)
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

