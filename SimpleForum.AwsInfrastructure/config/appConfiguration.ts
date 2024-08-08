import { config } from "dotenv";
import { join } from "path";

export interface AppConfiguration {
    ConnectionStrings__UserId: string;
    ASPNETCORE_URLS: string;
    Aws__CertificateArn: string;
    Aws__HostedZoneName: string;
    Aws__DataBucket: string;
    Aws__Region: string;
    Aws__AccountId: string;
    Aws__RepositoryName: string;
    Aws__ContainerName: string;
    GitHub__SecretName: string;
    GitHub__RepositoryName: string;
    GitHub__OwnerName: string;
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
        ConnectionStrings__UserId: parsedConfig.ConnectionStrings__UserId,
        ASPNETCORE_URLS: parsedConfig.ASPNETCORE_URLS,
        Aws__DataBucket: parsedConfig.Aws__DataBucket,

        GitHub__SecretName: parsedConfig.GitHub__SecretName,
        GitHub__RepositoryName: parsedConfig.GitHub__RepositoryName,
        GitHub__OwnerName: parsedConfig.GitHub__OwnerName,

        Aws__CertificateArn: parsedConfig.Aws__CertificateArn,
        Aws__HostedZoneName: parsedConfig.Aws__HostedZoneName,
        Aws__Region: parsedConfig.Aws__Region,
        Aws__AccountId: parsedConfig.Aws__AccountId,
        Aws__RepositoryName: parsedConfig.Aws__RepositoryName,
        Aws__ContainerName: parsedConfig.Aws__ContainerName,
    }

    return envVariableProps;
}
