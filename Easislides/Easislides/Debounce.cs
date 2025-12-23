using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Easislides
{
    public static class Debouncer
    {
        static ConcurrentDictionary<string, CancellationTokenSource> _tokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        public static void Debounce(string uniqueKey, Action action, int seconds)
        {
            var token = _tokens.AddOrUpdate(uniqueKey,
                (key) => //key not found - create new
            {
                    return new CancellationTokenSource();
                },
                (key, existingToken) => //key found - cancel task and recreate
            {
                    existingToken.Cancel(); //cancel previous
                return new CancellationTokenSource();
                }
            );

            Task.Delay(seconds * 1000, token.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                {
                    action();
                    _tokens.TryRemove(uniqueKey, out _);
                }
            }, token.Token);
        }
    }
}


//Debouncer.Debounce("Some-Unique-ID", () => SendEmails(), 5);
//부수적으로 문자열 키를 기반으로하기 때문에 인라인 람다를 사용할 수 있습니다.

//Debouncer.Debounce("Some-Unique-ID", () => 
//{
//    //do some work here
//}, 5);