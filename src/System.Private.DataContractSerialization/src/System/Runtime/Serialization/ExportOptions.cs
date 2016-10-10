﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;

namespace System.Runtime.Serialization
{
    public class ExportOptions
    {
        private Collection<Type> _knownTypes;
#if SUPPORT_SURROGATE
        IDataContractSurrogate dataContractSurrogate;

        public IDataContractSurrogate DataContractSurrogate
        {
            get { return dataContractSurrogate; }
            set { dataContractSurrogate = value; }
        }

        internal IDataContractSurrogate GetSurrogate()
        {
            return dataContractSurrogate;
        }
#endif

        public Collection<Type> KnownTypes
        {
            get
            {
                if (_knownTypes == null)
                {
                    _knownTypes = new Collection<Type>();
                }
                return _knownTypes;
            }
        }
    }
}
