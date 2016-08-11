﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace UpdateCreator
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null)
            {
                throw new ArgumentNullException("selectorExpression");
            }

            var body = selectorExpression.Body as MemberExpression;

            if (body == null)
            {
                throw new ArgumentException("The body must be a member expression");
            }

            this.OnPropertyChanged(body.Member.Name);
        }
        #endregion Implement INotifyPropertyChanged
    }
}