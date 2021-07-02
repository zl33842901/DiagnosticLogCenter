using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs
{
    public static class ExtMethod
    {
        private const string SplitString = ";-^45#24Si7&5$%;";
        public static string GetIndexName(this ICliEnvDate cliEnvDate)
        {
            var dbp = "logcenter-" + cliEnvDate.ClientName.ToLower() + "-" + cliEnvDate.EnvironmentName.ToLower() + "-" + cliEnvDate.HappenTime.ToString("yyyy.MM.dd");
            return dbp;
        }

        public static string GetUrl(this ICliEnvDate cliEnvDate)
        {
            var url = "/Log/Look/" + cliEnvDate.ClientName + "/" + cliEnvDate.EnvironmentName + "/" + cliEnvDate.HappenTime.ToString("yyyy-MM-dd");
            return url;
        }

        public static void PrepareLogForRead(this Log log)
        {
            //if (string.IsNullOrEmpty(log.AddtionsString))
            //    return;
            //bool allContentNull = log.Addtions.Length > 0 && !log.Addtions.Any(x => x.Content != null);
            //if (log.AddtionsString.Contains(SplitString) || allContentNull)
            //{
            //    var contents = log.AddtionsString.Split(new string[] { SplitString }, StringSplitOptions.None);
            //    for (var i = 0; i < log.Addtions.Length; i++)
            //    {
            //        if (contents.Length > i)
            //            log.Addtions[i].Content = contents[i];
            //    }
            //}
            if (string.IsNullOrEmpty(log.AddtionsString))
                return;
            bool allContentNull = log.Addtions.Length > 0 && !log.Addtions.Any(x => x.Content != null);
            if (log.AddtionsString.Contains(SplitString) || allContentNull)
            {
                var contents = log.AddtionsString.Split(new string[] { SplitString }, StringSplitOptions.None);
                log.Message = contents[0];
                log.StackTrace = contents.Length > 1 ? contents[1] : null;
                for (var i = 0; i < log.Addtions.Length; i++)
                {
                    var j = i + 2;
                    if (contents.Length > j)
                        log.Addtions[i].Content = contents[j];
                }
            }
        }


        public static Expression<Func<T, QueryContainer>> And<T>(this Expression<Func<T, QueryContainer>> first,
            Expression<Func<T, QueryContainer>> second)
        {
            if (first == null)
                return second;
            else if (second == null)
                return first;
            else
                return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, QueryContainer>> Or<T>(this Expression<Func<T, QueryContainer>> first,
            Expression<Func<T, QueryContainer>> second)
        {
            if (first == null)
                return second;
            else if (second == null)
                return first;
            else
                return first.Compose(second, Expression.OrElse);
        }

        public static Expression<Func<T, QueryContainer>> JoinAnd<T>(this IEnumerable<Expression<Func<T, QueryContainer>>> expressions)
        {
            Expression<Func<T, QueryContainer>> result = null;
            foreach (var exp in expressions)
            {
                result = result.And(exp);
            }
            return result;
        }

        public static Expression<Func<T, QueryContainer>> JoinOr<T>(this IEnumerable<Expression<Func<T, QueryContainer>>> expressions)
        {
            Expression<Func<T, QueryContainer>> result = null;
            foreach (var exp in expressions)
            {
                result = result.Or(exp);
            }
            return result;
        }

        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
            Func<Expression, Expression, Expression> merge)
        {
            var map = first.Parameters
                .Select((oldParam, index) => new { oldParam, newParam = second.Parameters[index] })
                .ToDictionary(p => p.newParam, p => p.oldParam);

            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
    }
    internal class ParameterRebinder : ExpressionVisitor
    {
        readonly Dictionary<ParameterExpression, ParameterExpression> _parameterMap;

        ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _parameterMap = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,
            Expression newParameters)
        {
            return new ParameterRebinder(map).Visit(newParameters);
        }

        protected override Expression VisitParameter(ParameterExpression newParameters)
        {
            if (_parameterMap.TryGetValue(newParameters, out var replacement))
            {
                newParameters = replacement;
            }

            return base.VisitParameter(newParameters);
        }
    }
}
