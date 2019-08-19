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
            Parse_jff_file();
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

        public void Parse_jff_file()
        {

            XmlDocument xml_doc = new XmlDocument();
            xml_doc.Load(file_path);

            foreach(XmlNode automaton in xml_doc.DocumentElement)
            {

                if(automaton.Name == "automaton")
                {
                    foreach(XmlNode state in automaton.ChildNodes)
                    {
                        if (state.Name == "state")
                        {
                            int id = Convert.ToInt32(state.Attributes[0].InnerText);
                            string name = state.Attributes[1].InnerText;
                            int x = 0, y = 0;

                            foreach (XmlNode location in state.ChildNodes)
                            {
                                if(location.Name == "x")
                                {
                                    x = Convert.ToInt32(Convert.ToDouble(location.FirstChild.Value)); //First Convert to Double because in file, this value is saved as double, and then to int
                                }

                                else if(location.Name == "y")
                                {
                                    y = Convert.ToInt32(Convert.ToDouble(location.FirstChild.Value)); //First Convert to Double because in file, this value is saved as double, and then to int

                                    //At this point, the state has been read, its x and y, name and id
                                    state_nodes.Add(new StateNode(id, x, y, name));

                                    break; //Break this foreach because the rest of the nodes are just garbage text
                                }
                            }
                        } else if(state.Name == "transition")
                        {
                            int from = 0, to = 0;
                            string read = "";

                            foreach(XmlNode transition in state.ChildNodes)
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
    }   
}
