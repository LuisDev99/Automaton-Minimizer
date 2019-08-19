using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Theory___Minimize_an_Automaton
{
    struct Coordinate
    {
        public int x, y;
        public Coordinate(int _x, int _y)
        {
            this.x = _x;
            this.y = _y;
        }
    }

    struct Transition
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
        public List<Transition> transition { get; set; }
        public string name { get; set; }
        public int id { get; set; }

        //useless and pointless constructor
        public StateNode(int _id, int x, int y, int to, string read, string _name)
        {
            this.coordinate = new Coordinate(x, y);
            this.transition = new List<Transition>();
            this.name = _name;
            this.id = _id;
        }

        public StateNode(int _id, int x, int y, string _name)
        {
            this.coordinate = new Coordinate(x, y);
            this.transition = new List<Transition>();
            this.name = _name;
            this.id = _id;
        }

        public void AddNodeTransition(int to, string read)
        {
            this.transition.Add(new Transition(to, read));
        }

        public StateNode()
        {

        }


    }
}
