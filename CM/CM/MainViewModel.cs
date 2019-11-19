using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CM.MVVM;

namespace CM
{
    public class MainViewModel : ObservableObject
    {
        private ObservableCollection<Person> _persons;

        public MainViewModel()
        {
            Persons = new ObservableCollection<Person>
            {
                new Person
                {
                    Name = "RS",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "OW",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "CT",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "SZ",
                    Position = new Position(0,0)
                }
            };
        }

        public ObservableCollection<Person> Persons
        {
            get => _persons;
            set => SetValue(ref _persons, value);
        }
    }
}
