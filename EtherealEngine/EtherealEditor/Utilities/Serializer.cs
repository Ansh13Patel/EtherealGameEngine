using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EtherealEditor.Utilities
{
    public static class Serializer
    {
        public static void ToFile<T> (T instance, string path)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Create);
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(fs, instance);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                fs?.Dispose();
            }
        }

        public static T FromFile<T> (string path)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open);
                var serializer = new DataContractSerializer(typeof(T));
                T instance = (T)serializer.ReadObject(fs);
                return instance;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return default(T);
            }
            finally
            {
                fs?.Dispose();
            }
        }
    }
}
