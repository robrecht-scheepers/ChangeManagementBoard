using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace CM.MVVM
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        private void InternalRaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            InternalRaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanges(params string[] properties)
        {
            foreach (var property in properties)
            {
                RaisePropertyChanged(property);
            }
        }

        protected void SetValue<T>(ref T refValue, T newValue, Action afterChangeAction = null, Action beforeChangeAction = null,[CallerMemberName] string propertyName = null)
        {
            if (Equals(refValue, newValue)) return;
            
            beforeChangeAction?.Invoke();

            refValue = newValue;

            InternalRaisePropertyChanged(propertyName);

            afterChangeAction?.Invoke();
        }

        private static string GetName<T>(Expression<Func<T>> prop)
        {
            var memberExpression = prop.Body as MemberExpression;
            if (memberExpression != null)
            {
                return memberExpression.Member.Name;
            }

            var unaryExpression = prop.Body as UnaryExpression;
            if (unaryExpression == null) throw new ArgumentException("Invalid property");

            var expression = unaryExpression.Operand as MemberExpression;
            if (expression == null) throw new ArgumentException("Invalid property");

            return expression.Member.Name;
        }
        
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
