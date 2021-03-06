/*
* sones GraphDB - Community Edition - http://www.sones.com
* Copyright (C) 2007-2011 sones GmbH
*
* This file is part of sones GraphDB Community Edition.
*
* sones GraphDB is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB. If not, see <http://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using sones.Library.NewFastSerializer;

namespace sones.Library.FastSerializer
{

    public class FillFastSerialTypeSurrogateList
    {

        #region Constructor(s)

        public FillFastSerialTypeSurrogateList()
        {
        }

        #endregion

        public void FillList()
        {
            FindAllTypes();
        }

        /// <summary>
        /// Try to add type to be serialized.
        /// </summary>
        /// <param name="_Type">type to be serialized</param> 
        public static void TryAddType(Type _Type)
        {
            if (_Type.IsAbstract)
                return;

            if (_Type.IsInterface)
                return;

            // Not a *_Accessor class created from the tests
            // maybe we can get here a problem with derived types
           /* if (_Type.Module.ScopeName == "GraphDB_Accessor")
                return;

            if (typeof(IFastSerializationTypeSurrogate).IsAssignableFrom(_Type))
            {

                IFastSerializationTypeSurrogate SurrogateType = (IFastSerializationTypeSurrogate)Activator.CreateInstance(_Type);

                var existingType = SerializationWriter.findSurrogateForType(SurrogateType.TypeCode);
                if (existingType != null && existingType.GetType() != _Type)
                {
                    throw new sones.Library.Exceptions.FastSerializeSurrogateTypeCodeExistException(
                        "Could not add [" + _Type.Name + "] SurrogateForType already exists with typeCode " + SurrogateType.TypeCode
                        + "[" + SerializationWriter.findSurrogateForType(SurrogateType.TypeCode).GetType().Name + "]");
                }

                SerializationWriter.AddSurrogateType(SurrogateType);

            }*/
        }
        
        private void FindAllTypes()
        {

            var files = new List<String>();
            files.AddRange(Directory.GetFiles(".", "*.dll"));
            files.AddRange(Directory.GetFiles(".", "*.exe"));

            foreach (var actualFile in files)
            {

                var fileInfo = new FileInfo(actualFile);
                Type[] allTypes = null;

                //Debug.WriteLine(String.Format("{0} : ", actualFile));
                try
                {
                    allTypes = Assembly.LoadFrom(fileInfo.FullName).GetTypes();
                }
                catch (ReflectionTypeLoadException rtlex)
                {
                    String exception = String.Empty;

                    foreach(var item in rtlex.LoaderExceptions)
                    {
                        exception += item.ToString() + Environment.NewLine + Environment.NewLine;
                    }

                    Console.WriteLine("Could not get types from file [" + fileInfo.Name + "]: " + exception);
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not get types from file [" + fileInfo.Name + "]: " + ex.ToString());
                    continue;
                }

                foreach (var _Type in allTypes)
                {
                    try
                    {

                        TryAddType(_Type);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(fileInfo.Name + " failed for type [" + _Type + "]: " + ex.Message);
                    }
                }

            }

        }

    }

}
