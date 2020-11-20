using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
        private AsyncRelayCommand _addPersonCommand;
        private ObservableCollection<string> _projects;
        private string _selectedProject;
        private string _newProjectName;
        private AsyncRelayCommand _addProjectCommand;

        public MainViewModel(DbRepository repository)
        {
            _repository = repository;
            Projects = new ObservableCollection<string>(_repository.GetProjects().OrderBy(x => x));
            SelectedProject = Projects.FirstOrDefault();
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

            LoadProjectParticipants().Wait();
        }

        private async Task LoadProjectParticipants()
        {
            Persons = new ObservableCollection<Person>((await _repository.GetParticipants(SelectedProject)).OrderBy(x => x.Name));
            foreach (var person in Persons)
            {
                person.PropertyChanged += async (s, a) => await _repository.UpdateParticipant(SelectedProject, person.Name,
                    person.Position.Phase, person.Position.Resistance);
            }
        }

        public string NewProjectName
        {
            get => _newProjectName;
            set => SetValue(ref _newProjectName, value);
        }

        public AsyncRelayCommand AddProjectCommand => _addProjectCommand ?? (_addProjectCommand = new AsyncRelayCommand(AddProject, CanExecuteAddProject));

        private async Task AddProject()
        {
            await _repository.AddProject(NewProjectName);
            await _repository.AddParticipant(NewProjectName, ProjectNamePlaceholder, 1, 0);
            Projects = new ObservableCollection<string>(_repository.GetProjects().OrderBy(x => x));
            if (Projects.Contains(NewProjectName))
                SelectedProject = NewProjectName;
            NewProjectName = "";
        }

        private bool CanExecuteAddProject()
        {
            return !string.IsNullOrEmpty(NewProjectName) && !Projects.Contains(NewProjectName);
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

        public AsyncRelayCommand AddPersonCommand => _addPersonCommand ?? (_addPersonCommand = new AsyncRelayCommand(AddPerson, CanExecuteAddPerson));

        private async Task AddPerson()
        {
            await _repository.AddParticipant( SelectedProject, NewPersonName, 0, 0);
            await LoadProjectParticipants();

            NewPersonName = "";
        }

        private bool CanExecuteAddPerson()
        {
            return !string.IsNullOrEmpty(NewPersonName) && !Persons.Any(x => x.Name == NewPersonName);
        }
    }
}
