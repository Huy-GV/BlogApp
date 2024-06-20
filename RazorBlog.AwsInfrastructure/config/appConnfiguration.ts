import { DotenvParseOutput, config } from "dotenv";
import { join } from "path";

interface AppConfiguration {
    SeedUserPassword: string;
    DatabaseUserId: string;
    DatabaseName: string;
    SqlServerPassword: string;
    
    AspNetCoreKestrelCertPassword: string;
    AspNetCoreKestrelCertPath: string;
    AspNetCorUrls: string;
    AspNetCoreHttpsPort: number;
  
    AwsSecretKey: string;
    AwsAccessKey: string;
    AwsS3BucketName: string;
  
    RawConfig: DotenvParseOutput;
}
  
export function parseEnvFile(): AppConfiguration | null {
    const envFilePath = join(__dirname, 'aws.env');
    console.log(`Reading environment variables from '${envFilePath}'`)
    const envConfigResult = config({
        path: envFilePath
    });

    if (envConfigResult.error) {
        console.error(envConfigResult);
        return null;
    }

    const parsedConfig = envConfigResult.parsed;
    if (parsedConfig === undefined || parsedConfig === null) {
        return null;
    }

    if (Object.values(parsedConfig).some(x => x === undefined || x === null)) {
        return null;
    }

    const envVariableProps: AppConfiguration = {
        SeedUserPassword: parsedConfig.SeedUser__Password,
        DatabaseUserId: parsedConfig.Database__UserId,
        DatabaseName: parsedConfig.Database__Name,
        SqlServerPassword: parsedConfig.SqlServer__Password,
        AspNetCoreKestrelCertPassword: parsedConfig.ASPNETCORE_Kestrel__Certificates__Default__Password,
        AspNetCoreKestrelCertPath: parsedConfig.ASPNETCORE_Kestrel__Certificates__Default__Path,
        AwsSecretKey: parsedConfig.Aws__SecretKey,
        AwsAccessKey: parsedConfig.Aws__S3__BucketName,
        AwsS3BucketName: parsedConfig.Aws__AccessKey,
        AspNetCoreHttpsPort: Number.parseInt(parsedConfig.ASPNETCORE_HTTPS_PORT),
        AspNetCorUrls: parsedConfig.ASPNETCORE_URLS,

        RawConfig: parsedConfig
    }

    return envVariableProps;
}