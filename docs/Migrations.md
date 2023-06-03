# Adding a migration
```
cd Sinance.Storage
dotnet ef --startup-project ../Sinance.Web migrations add <migrationname> -c SinanceContext
```

# Removing the last migration
```
cd Sinance.Storage
dotnet ef migrations remove
```