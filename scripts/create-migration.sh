if [ -z $1 ]
then
    echo "Please, name the migration to be created. e.g. '$ sh create-migration.sh some_new_migration'"
else
    EF_CORE_PROJECT_PATH=../src/PaymentGateway.Infrastructure/PaymentGateway.Infrastructure.csproj
    ENTRY_POINT_PROJECT_PATH=../src/PaymentGateway.Web/PaymentGateway.Web.csproj

    dotnet dotnet-ef migrations add $1 -p $EF_CORE_PROJECT_PATH -s $ENTRY_POINT_PROJECT_PATH --verbose
fi