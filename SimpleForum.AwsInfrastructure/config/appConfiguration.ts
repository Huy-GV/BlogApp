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
    GitHub__SecretName: string;
    GitHub__RepositoryName: string;
    GitHub__OwnerName: string;
    Aws__Region: string;
    Aws__AccountId: string;
    Aws__RepositoryName: string;
    Aws__ContainerName: string;
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
        Aws__HostedZoneName: parsedConfig.Aws__HostedZoneName,
        GitHub__SecretName: parsedConfig.GitHub__SecretName,
        GitHub__RepositoryName: parsedConfig.GitHub__RepositoryName,
        GitHub__OwnerName: parsedConfig.GitHub__OwnerName,
        Aws__Region: parsedConfig.AWS_REGION,
        Aws__AccountId: parsedConfig.AWS_ACCOUNT_ID,
        Aws__RepositoryName: parsedConfig.REPOSITORY_NAME,
        Aws__ContainerName: parsedConfig.CONTAINER_NAME,
    }

    return envVariableProps;
}
