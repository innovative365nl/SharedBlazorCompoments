using System.ComponentModel;
namespace ExampleApp.Pages;
public partial class ExampleText
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task ChangeName()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        _person.Name = "Jane Smith";
        _person.Age = 31;

    }
    internal sealed class Person : INotifyPropertyChanged
    {
        private string _name = "John Doe";
        private int _age = 30; 
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
        
        public int Age // Add Age property
        {
            get => _age;
            set
            {
                if (_age != value)
                {
                    _age = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
                }
            }
        }
    }
    
    private readonly Person _person = new Person();
}