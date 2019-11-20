﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CM.Data;
using CM.MVVM;

namespace CM
{
    public class MainViewModel : ObservableObject
    {
        private readonly DbRepository _repository;
        private const string ProjectNamePlaceholder = "_";
        private ObservableCollection<Person> _persons;
        private string _newPersonName;
        private RelayCommand _addPersonCommand;
        private ObservableCollection<string> _projects;
        private string _selectedProject;

        public MainViewModel(DbRepository repository)
        {
            _repository = repository;

            Persons = new ObservableCollection<Person>
            {
                new Person
                {
                    Name = "_",
                    Position = new Position(1,0)
                },
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
                },
                new Person
                {
                    Name = "NBE",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "AKL",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "CST",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "MH",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "SMA",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "AKF",
                    Position = new Position(0,0)
                },
                new Person
                {
                    Name = "HS",
                    Position = new Position(0,0)
                },
            };
        }

        public ObservableCollection<string> Projects
        {
            get => _projects;
            set => SetValue(ref _projects, value);
        }

        public string SelectedProject
        {
            get => _selectedProject;
            set => SetValue(ref _selectedProject, value, SelectedProjectChanged);
        }

        private void SelectedProjectChanged()
        {
            if (string.IsNullOrEmpty(SelectedProject))
            {
                Persons = null;
                return;
            }

            var task = _repository.GetParticipants(SelectedProject);
            task.Wait();
            Persons = new ObservableCollection<Person>(task.Result);
        }

        public ObservableCollection<Person> Persons
        {
            get => _persons;
            set => SetValue(ref _persons, value);
        }

        public string NewPersonName
        {
            get => _newPersonName;
            set => SetValue(ref _newPersonName , value);
        }

        public RelayCommand AddPersonCommand => _addPersonCommand ?? (_addPersonCommand = new RelayCommand(AddPerson, CanExecuteAddPerson));

        private void AddPerson()
        {
            Persons.Add(new Person
            {
                Name = NewPersonName,
                Position = new Position(0,0)
            });
            NewPersonName = "";
        }

        private bool CanExecuteAddPerson()
        {
            return !string.IsNullOrEmpty(NewPersonName) && !Persons.Any(x => x.Name == NewPersonName);
        }
    }
}
