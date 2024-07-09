import { config } from "dotenv";
import { join } from "path";

export interface AppConfiguration {
    SeedUser__Password: string;
    Database__UserId: string;
    Database__Name: string;
    SqlServer__Password: string;
    ASPNETCORE_URLS: string;
    Aws__DataBucket: string;
    Aws__CertificateArn: string;
    Aws__HostedZoneName: string;
    Aws__HostedZoneId: string;
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
        SeedUser__Password: parsedConfig.SeedUser__Password,
        Database__UserId: parsedConfig.Database__UserId,
        Database__Name: parsedConfig.Database__Name,
        SqlServer__Password: parsedConfig.SqlServer__Password,
        Aws__DataBucket: parsedConfig.Aws__DataBucket,
        ASPNETCORE_URLS: parsedConfig.ASPNETCORE_URLS,
        Aws__CertificateArn: parsedConfig.Aws__CertificateArn,
        Aws__HostedZoneId: parsedConfig.Aws_HostedZoneId,
        Aws__HostedZoneName: parsedConfig.Aws__HostedZoneName
    }

    return envVariableProps;
}
