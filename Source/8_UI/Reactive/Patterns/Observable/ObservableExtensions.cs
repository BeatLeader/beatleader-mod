using System;
using System.Linq.Expressions;

namespace BeatLeader.UI.Reactive {
    internal static class ObservableExtensions {
        public static T WithListener<T, TValue>(
            this T host,
            Expression<Func<T, TValue>> expression,
            Action<TValue> listener
        ) where T : IObservableHost {
            if (expression.Body is not MemberExpression memberExpression) {
                throw new ArgumentException("The expression is not a member access expression");
            }
            var name = memberExpression.Member.Name;
            host.AddCallback(name, listener);
            return host;
        }
    }
}