﻿using System.Collections.Generic;
using ABC.Model.Users;
using ABC.Model.Primitives;

namespace ABC.Model
{
    /// <summary>
    /// Activity Base Class
    /// </summary>
    public class Activity : Noo, IActivity
    {
        #region Constructors

        public Activity()
        {
            BaseType = typeof(IActivity).Name;
            InitializeProperties();
        }

        #endregion

        #region Initializers

        private void InitializeProperties()
        {
            Actions = new List<Action>();
            Participants = new List<User>();
            Meta = new Metadata();
            Resources =  new List<Resource>();
        }

        #endregion

        #region Properties

        private User owner;
        public User Owner
        {
            get { return owner; }
            set
            {
                owner = value;
                OnPropertyChanged("owner");
            }
        }
        private List<User> participants;
        public List<User> Participants
        {
            get { return participants; }
            set
            {
                participants = value;
                OnPropertyChanged("participants");
            }
        }
        private List<Action> actions;
        public List<Action> Actions
        {
            get { return actions; }
            set
            {
                actions = value;
                OnPropertyChanged("actions");
            }
        }
        private Metadata meta;
        public Metadata Meta
        { 
            get{return meta;}
            set
            {
                meta=value;
                OnPropertyChanged("meta");
            }
        }
        private List<Resource> resources;
        public List<Resource> Resources
        {
            get { return resources; }
            set
            {
                resources = value;
                OnPropertyChanged("resouces");
            }
        }

        #endregion

        #region Public Methods

        public List<Resource> GetResources()
        {
            return Resources;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Activity act)
        {
            return Id == act.Id;
        }

        #endregion
    }
}