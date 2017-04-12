using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DesktopTestData
{
    [DataContract]
    public class ObjectContainer
    {
        [DataMember]
        private object data;

        [DataMember]
        private object data2;

        public ObjectContainer(object input)
        {
            data = input;
            data2 = data;
        }

        public object Data
        {
            get { return data; }
        }

        public object Data2
        {
            get { return data2; }
        }
    }

    [DataContract]
    public class EnumStructContainer
    {
        [DataMember]
        public object p1 = new VT(10);

        [DataMember]
        public object p2 = new NotSer();

        [DataMember]

        public object[] enumArrayData = new object[] { MyEnum1.red, MyEnum1.black, MyEnum1.blue, Seasons1.Autumn, Seasons2.Spring };

        [DataMember]
        public object p3 = new MyStruct();

    }
}
