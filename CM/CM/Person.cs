using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CM.MVVM;

namespace CM
{
    public class Person : ObservableObject
    {
        private string _name;
        private Position _position;

        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public Position Position
        {
            get => _position;
            set => SetValue(ref _position, value);
        }
    }
}
