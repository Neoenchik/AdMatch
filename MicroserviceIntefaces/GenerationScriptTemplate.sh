#!/bin/bash
# Usage:
#   ./GenerationScriptTemplate.sh <MicroserviceApiName> [--format] [--optionalPropertiesAsNullable]
#
# Arguments:
#   <MicroserviceApiName> Name prefix for api client class generation
#
# Options:
#   --format                        The swagger file format. Default is 'yaml'
#   --optionalPropertiesAsNullable  Value for the NSwag option GenerateOptionalPropertiesAsNullable. Default is false
#
# Example:
#   ./GenerationScriptTemplate.sh SampleApi --format json --optionalPropertiesAsNullable true
#

ENTITY=${1}

# Helper for parsing named parameters
while [ $# -gt 0 ]; do
   if [[ $1 == *"--"* ]]; then
        param="${1/--/}"
        declare $param="$2"
   fi
  shift
done

SPECIFICATION_FORMAT=${format:-yaml}
INTERFACE=$ENTITY
INTERFACE+="Controller"
INPUTFILE=$INTERFACE.openapi.$SPECIFICATION_FORMAT

MicroserviceNameSuffix=${microserviceNameSuffix:-"MicroserviceTemplate"}
ProjectPrefix=${projectPrefix:-"EFS"}
ControllersProject=${controllersProject:-"$ProjectPrefix.$MicroserviceNameSuffix"}
OutputForApiController=${outputForApiController:-"../$ControllersProject/Controllers/${INTERFACE}Base.cs"}
Namespace=${namespace:-"$ControllersProject".Controllers}
AdditionalNamespaceUsages=${additionalNamespaceUsages:-}
OPTIONAL_AS_NULLABLE=${optionalPropertiesAsNullable:-false}

if [ ! -f "$INPUTFILE" ]; then
  INPUTFILE=$INTERFACE.swagger.$SPECIFICATION_FORMAT
  if [ ! -f "$INPUTFILE" ]; then
      INPUTFILE=$INTERFACE.$SPECIFICATION_FORMAT
  fi
fi

Input=${input:-$INPUTFILE}

echo "Generating API controller for $ENTITY from $Input using NSwag CLI..."

# Проверяем, установлен ли NSwag CLI
if ! command -v nswag &> /dev/null
then
    echo "NSwag CLI не найден. Устанавливаю глобально..."
    dotnet tool install --global NSwag.ConsoleCore
    export PATH="$PATH:$HOME/.dotnet/tools"
fi

# Генерация контроллера
nswag openapi2cscontroller \
    /Input:$Input \
    /ClassName:$ENTITY \
    /Output:$OutputForApiController \
    /Namespace:$Namespace \
    /AdditionalNamespaceUsages:$AdditionalNamespaceUsages \
    /ControllerStyle:Abstract \
    /ControllerTarget:AspNetCore \
    /ControllerBaseClass:Microsoft.AspNetCore.Mvc.ControllerBase \
    /GenerateModelValidationAttributes:true \
    /RouteNamingStrategy:None \
    /OperationGenerationMode:MultipleClientsFromOperationId \
    /GenerateOptionalParameters:true \
    /UseActionResultType:true \
    /ExcludedTypeNames:"FileParameter,FileResponse" \
    /GenerateNullableReferenceTypes:false \
    /GenerateOptionalPropertiesAsNullable:$OPTIONAL_AS_NULLABLE \
    /GenerateJsonMethods:false \
    /JsonLibrary:NewtonsoftJson \
    /UseCancellationToken:true \
    /DateType:System.DateTime \
    /DateTimeType:System.DateTimeOffset \
    /TimeType:System.TimeSpan \
    /TimeSpanType:System.TimeSpan \
    /ArrayType:System.Collections.Generic.List \
    /ArrayInstanceType:System.Collections.Generic.List \
    /ArrayBaseType:System.Collections.Generic.List \
    /ResponseArrayType:System.Collections.Generic.IEnumerable \
    /ParameterArrayType:System.Collections.Generic.IEnumerable

if [ $? -ne 0 ]; then
  echo "An error occurred. The script has stopped."
  read -p "Press Enter to exit..."
  exit 1
fi
