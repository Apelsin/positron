using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Positron
{
    [DataContract]
    public abstract class ContractElementBase
    {
        #region Virtual Serialization Wrapper Functions
        [OnDeserialized]
        internal void _OnDeserialized(StreamingContext context)
        {
            OnDeserialized(context);
        }
        internal virtual void OnDeserialized(StreamingContext context)
        {

        }
        [OnSerialized]
        internal void _OnSerialized(StreamingContext context)
        {
            OnSerialized(context);
        }
        internal virtual void OnSerialized(StreamingContext context)
        {

        }
        [OnDeserializing]
        internal void _OnDeserializing(StreamingContext context)
        {
            OnDeserializing(context);
        }
        internal virtual void OnDeserializing(StreamingContext context)
        {

        }
        [OnSerializing]
        internal void _OnSerializing(StreamingContext context)
        {
            OnSerializing(context);
        }
        internal virtual void OnSerializing(StreamingContext context)
        {

        }
        #endregion
    }
    [DataContract]
    public abstract class ContractElement : ContractElementBase
    {
        /// <summary>
        /// Unique string Id for this item
        /// </summary>
        [DataMember]
        internal string ElementId;
        
        public ContractElement()
        {
            ElementId = GetInternalElementId();
        }
        internal virtual string GetInternalElementId()
        {
            return GetInternalElementId(this);
        }
        internal virtual string GetInternalElementId(object reference)
        {
            return this.GetType().FullName + "#" + reference.GetHashCode().ToString();
        }
    }
}
