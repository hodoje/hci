using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class NetworkViewViewModel : BindableBase
    {
        // ObservableCollection represents a dynamic data collection that provides notifications when items get added, removed or when the whole list is refreshed
        // Essentially, it works like a regular collection, except that it implements the interfaces "INotifyCollectionChanged" and "INotifyPropertyChanged"
        // As such, it is very useful when we want to know when the collection has changed, it allows the code outside the collection to be aware of when changes to the collection occur
        // An event is triggered that will tell the user that entries have been added/removed or moved
        public ObservableCollection<Road> Roads { get; set; }

        public NetworkViewViewModel()
        {
            LoadRoads();
        }

        public void LoadRoads()
        {
            ObservableCollection<Road> roads = new ObservableCollection<Road>();

            roads.Add(new Road{Id = 1, Label = "M70", Type = Types.ListOfTypes[0], Value = 2000} );
            roads.Add(new Road { Id = 2, Label = "M75", Type = Types.ListOfTypes[1], Value = 3000 });

            Roads = roads;
        }
    }
}
