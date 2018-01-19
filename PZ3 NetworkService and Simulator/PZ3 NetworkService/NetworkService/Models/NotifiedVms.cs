using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Models
{
    public class NotifiedVms : BindableBase
    {
        private NotifiedVms()
        {
            NotifiedVmsList = new List<INotify>();
            ListOfActions = new List<Action>();
        }

        public List<INotify> NotifiedVmsList { get; set; }

        public List<Action> ListOfActions { get; set; }

        private static NotifiedVms NV { get; set; }

        public static NotifiedVms Instance
        {
            get
            {
                if (NV == null)
                {
                    NV = new NotifiedVms();
                }
                return NV;
            }
        }

        private Road _currentRoad;

        public Road CurrentRoad
        {
            get { return _currentRoad; }
            set
            {
                if (_currentRoad != value)
                {
                    _currentRoad = value;
                    OnPropertyChanged("CurrentRoad");
                }
            }
        }

        public void Register(INotify obj)
        {
            if (!NotifiedVmsList.Contains(obj))
            {
                NotifiedVmsList.Add(obj);
            }
        }

        public void RegisterAction(Action act)
        {
            if (!ListOfActions.Contains(act))
            {
                ListOfActions.Add(act);
            }
        }

        public void NotifyAll(Road changedRoad)
        {
            foreach (var vm in NotifiedVmsList)
            {
                vm.Notify(changedRoad);
                CurrentRoad = changedRoad;
            }
            foreach (var act in ListOfActions)
            {
                act.Invoke();
            }
        }
    }
}
