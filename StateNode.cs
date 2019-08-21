using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Theory___Minimize_an_Automaton
{
    struct Coordinate
    {
        public double x, y;
        public Coordinate(double _x, double _y)
        {
            this.x = _x;
            this.y = _y;
        }
    }

    public struct Transition
    {
        public int to { get; set; }
        public string read { get; set; }

        public Transition(int _to, string _read)
        {
            this.to = _to;
            this.read = _read;
        }
    }

    class StateNode
    {
        public Coordinate coordinate { get; set; }
        public List<Transition> transitions { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public bool isInitial { get; set; }
        public bool isFinal { get; set; }
        //useless and pointless constructor
        public StateNode(int _id, double x, double y, int to, string read, string _name, bool _isInitial, bool _isFinal)
        {
            this.coordinate = new Coordinate(x, y);
            this.transitions = new List<Transition>();
            this.name = _name;
            this.id = _id;
            this.isInitial = _isInitial;
            this.isFinal = _isFinal;
        }

        public StateNode(int _id, double x, double y, string _name, bool _isInitial, bool _isFinal)
        {
            this.coordinate = new Coordinate(x, y);
            this.transitions = new List<Transition>();
            this.name = _name;
            this.id = _id;
            this.isInitial = _isInitial;
            this.isFinal = _isFinal;
        }

        public void AddNodeTransition(int to, string read)
        {
            this.transitions.Add(new Transition(to, read));
        }

        public StateNode()
        {

        }


    }
}
