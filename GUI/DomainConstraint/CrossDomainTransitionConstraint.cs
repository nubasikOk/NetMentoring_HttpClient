﻿using NetMentoring_HttpClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.DomainConstraint
{
    class CrossDomainTransitionConstraint : IDomainConstraint
    {
        private readonly Uri parentUri;
        private readonly CrossDomainTransition availableTransition;
        public CrossDomainTransitionConstraint(CrossDomainTransition availableTransition, Uri parentUri)
        {
            switch (availableTransition)
            {
                case CrossDomainTransition.All:
                case CrossDomainTransition.CurrentDomainOnly:
                case CrossDomainTransition.DescendantUrlsOnly:
                    this.availableTransition = availableTransition;
                    this.parentUri = parentUri;
                    break;
                default:
                    throw new ArgumentException($"Unknown transition type: {availableTransition}");
            }
        }

        public bool IsAcceptable(Uri uri)
        {
            switch (availableTransition)
            {
                case CrossDomainTransition.All:
                    return true;

                case CrossDomainTransition.CurrentDomainOnly:
                    return parentUri.DnsSafeHost == uri.DnsSafeHost;
                    
                case CrossDomainTransition.DescendantUrlsOnly:
                    return parentUri.IsBaseOf(uri);
                   
            }

            return false;
        }
    }
}
