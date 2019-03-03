﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDEdit.Utilities.Attributes;
using GDEdit.Utilities.Objects.General;
using GDEdit.Utilities.Enumerations.GeometryDash;

namespace GDEdit.Utilities.Objects.GeometryDash.LevelObjects.Triggers
{
    /// <summary>Represents a trigger.</summary>
    public abstract class Trigger : ConstantIDObject
    {
        /// <summary>Contains the <seealso cref="bool"/> values of the trigger. Indices 0, 1, 2 are reserved for Touch Triggered, Spawn Triggered and Multi Trigger respectively.</summary>
        protected BitArray8 TriggerBools = new BitArray8();
        
        /// <summary>The Touch Triggered property of the trigger.</summary>
        [ObjectStringMappable(ObjectParameter.TouchTriggered)]
        public bool TouchTriggered
        {
            get => TriggerBools[0];
            set => TriggerBools[0] = value;
        }
        /// <summary>The Spawn Triggered property of the trigger.</summary>
        [ObjectStringMappable(ObjectParameter.SpawnTriggered)]
        public bool SpawnTriggered
        {
            get => TriggerBools[1];
            set => TriggerBools[1] = value;
        }
        /// <summary>The Multi Trigger property of the trigger.</summary>
        [ObjectStringMappable(ObjectParameter.MultiTrigger)]
        public bool MultiTrigger
        {
            get => TriggerBools[2];
            set => TriggerBools[2] = value;
        }

        /// <summary>Initializes a new instance of the <seealso cref="Trigger"/> class.</summary>
        public Trigger() : base() { }
        /// <summary>Initializes a new instance of the <seealso cref="Trigger"/> class.</summary>
        /// <param name="touchTriggered">The Touch Triggered property of the trigger.</param>
        public Trigger(bool touchTriggered)
        {
            TouchTriggered = touchTriggered;
        }
        /// <summary>Initializes a new instance of the <seealso cref="Trigger"/> class.</summary>
        /// <param name="spawnTriggered">The Spawn Triggered property of the trigger.</param>
        /// <param name="multiTrigger">The Multi Trigger property of the trigger.</param>
        public Trigger(bool spawnTriggered, bool multiTrigger)
        {
            SpawnTriggered = spawnTriggered;
            MultiTrigger = multiTrigger;
        }
        
        /// <summary>Adds the cloned instance information and returns the cloned instance.</summary>
        /// <param name="cloned">The cloned instance to add the information to.</param>
        protected override GeneralObject AddClonedInstanceInformation(GeneralObject cloned)
        {
            var c = cloned as Trigger;
            c.TouchTriggered = TouchTriggered;
            c.SpawnTriggered = SpawnTriggered;
            c.MultiTrigger = MultiTrigger;
            return base.AddClonedInstanceInformation(c);
        }
    }
}
