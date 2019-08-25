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
        public string destination_name { get; set; }

        public Transition(int _to, string _read, string _destination_name)
        {
            this.to = _to;
            this.read = _read;
            this.destination_name = _destination_name;
        }
    }

    public struct MergeInfo
    {
        public string merge_with { get; set; }
        public string name_before_merge { get; set; }
        public bool isMerge { get; set; }

        public MergeInfo(string _merge_with, string _name_before_merge, bool _isMerge)
        {
            this.name_before_merge = _name_before_merge;
            this.merge_with = _merge_with;
            this.isMerge = _isMerge;
        }
    }

    class StateNode
    {
        public MergeInfo mergeInfo { get; set; }
        public Coordinate coordinate { get; set; }
        public List<Transition> transitions { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public bool isInitial { get; set; }
        public bool isFinal { get; set; }
        //useless and pointless constructor
        public StateNode(int _id, double x, double y, int to, string read, string _name, bool _isInitial, bool _isFinal)
        {
            this.mergeInfo = new MergeInfo();
            this.coordinate = new Coordinate(x, y);
            this.transitions = new List<Transition>();
            this.name = _name;
            this.id = _id;
            this.isInitial = _isInitial;
            this.isFinal = _isFinal;
        }

        public StateNode(int _id, double x, double y, string _name, bool _isInitial, bool _isFinal)
        {
            this.mergeInfo = new MergeInfo();
            this.coordinate = new Coordinate(x, y);
            this.transitions = new List<Transition>();
            this.name = _name;
            this.id = _id;
            this.isInitial = _isInitial;
            this.isFinal = _isFinal;
        }

        public void AddNodeTransition(int to, string read, string destination_name)
        {
            this.transitions.Add(new Transition(to, read, destination_name));
        }

        public StateNode()
        {

        }


    }
}
