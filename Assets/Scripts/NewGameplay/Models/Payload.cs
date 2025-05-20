using UnityEngine;
using NewGameplay.Enums;

namespace NewGameplay.Models
{
    public class Payload
    {
        public PayloadType Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }

        public Payload(PayloadType type, string name, string description)
        {
            Type = type;
            Name = name;
            Description = description;
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
} 