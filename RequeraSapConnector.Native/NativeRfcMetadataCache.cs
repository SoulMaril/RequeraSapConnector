﻿using SAP.Middleware.Connector;
using RequeraSapConnector.Metadata;
using RequeraSapConnector.Types;
using System;
using System.Collections.Generic;
using RequeraSapConnector.Native.Extension;

namespace RequeraSapConnector.Native
{
    public class NativeRfcMetadataCache : RfcMetadataCache
    {
        private NativeSapRfcConnection connection;
        public NativeRfcMetadataCache(NativeSapRfcConnection connection)
        {
            this.connection = connection;
        }

        protected override FunctionMetadata LoadFunctionMetadata(string functionName)
        {
            var remoteFunctionMetadata = this.connection.Repository.GetFunctionMetadata(functionName);

            List<ParameterMetadata> inputParameters = new List<ParameterMetadata>();
            List<ParameterMetadata> outputParameters = new List<ParameterMetadata>();

            for (int i = 0; i < remoteFunctionMetadata.ParameterCount; i++)
            {
                StructureMetadata structureMetadata = null;
                if (remoteFunctionMetadata[i].DataType == RfcDataType.STRUCTURE)
                {
                    string structureName = remoteFunctionMetadata[i].ValueMetadataAsStructureMetadata.Name;
                    structureMetadata = this.GetStructureMetadata(structureName);
                }
                else if (remoteFunctionMetadata[i].DataType == RfcDataType.STRUCTURE)
                {
                    string structureName = remoteFunctionMetadata[i].ValueMetadataAsTableMetadata.LineType.Name;
                    structureMetadata = this.GetStructureMetadata(structureName);
                }

                var parameter = new ParameterMetadata(remoteFunctionMetadata[i].Name, remoteFunctionMetadata[i].GetAbapDataType(), structureMetadata);
                switch (remoteFunctionMetadata[i].Direction)
                {
                    case RfcDirection.EXPORT:
                        outputParameters.Add(parameter);
                        break;
                    case RfcDirection.IMPORT:
                        inputParameters.Add(parameter);
                        break;
                    case RfcDirection.TABLES:
                    case RfcDirection.CHANGING:
                        inputParameters.Add(parameter);
                        outputParameters.Add(parameter);
                        break;
                    default:
                        break;
                }
            }

            return new FunctionMetadata(functionName, inputParameters, outputParameters);
        }

        protected override StructureMetadata LoadStructureMetadata(string structureName)
        {
            var remoteStructureMetadata = this.connection.Repository.GetStructureMetadata(structureName);

            List<FieldMetadata> fields = new List<FieldMetadata>();
            for (int i = 0; i < remoteStructureMetadata.FieldCount; i++)
            {
                string fieldName = remoteStructureMetadata[i].Name;
                AbapDataType fieldType = remoteStructureMetadata[i].GetAbapDataType();
                fields.Add(new FieldMetadata(fieldName, fieldType));
            }

            return new StructureMetadata(structureName, fields);
        }
    }
}
