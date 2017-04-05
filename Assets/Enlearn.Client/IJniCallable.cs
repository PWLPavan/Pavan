using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Enlearn.Client
{
    public interface IJniCallable
    {
        string Call(string commandName, params string[] args);
    }
}
