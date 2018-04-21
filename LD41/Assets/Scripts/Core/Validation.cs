using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class Validation
    {
        public bool Condition { get; set; }
        public string Message { get; set; }
        public bool Error { get; set; }

        public Validation(bool condition, string message, bool error = false)
        {
            Condition = condition;
            Message = message;
            Error = error;
        }

        public static implicit operator bool(Validation validation)
        {
            if (validation.Condition) return true;
            if (validation.Error)
            {
                Debug.LogError(validation.Message);
            }
            else
            {
                Debug.LogWarning(validation.Message);
            }
            return false;
        }
    }
}
