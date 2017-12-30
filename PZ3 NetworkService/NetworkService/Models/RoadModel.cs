using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService
{
    // INotifyPropertyChanged interface is used to notify clients, typically binding clients, that a property value has changed
    public class Road : INotifyPropertyChanged
    {
        private int _id;
        private string _label;
        private Type _type;
        private double _value;

        public int Id { get; set; }
        public string Label { get; set; }
        public Type Type { get; set; }

        public double Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    // Call RaisePropertyChanged when the property is updated
                    RaisePropertyChanged("Value");
                }
            }
        }

        // Declare the PropertyChanged event
        // Method that will handle the PropertyChanged event raised when a property is changed on a component
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                // PropertyChangedEventArgs provide data for the PropertyChanged event
                // A PropertyChanged event is raised when a property is changed on a component
                // A PropertyChangedEventArgs object specifies the name of the property that changed 
                // (it provides the PropertyName property to get the name of the property that changed)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
