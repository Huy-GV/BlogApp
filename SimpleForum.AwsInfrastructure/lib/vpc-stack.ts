import * as cdk from 'aws-cdk-lib';
import { ISecurityGroup, IVpc, Peer, Port, SecurityGroup, SubnetType, Vpc } from 'aws-cdk-lib/aws-ec2';
import { Construct } from 'constructs';

export class VpcStack extends cdk.Stack {
	readonly vpc: IVpc
	readonly webTierSecurityGroup: ISecurityGroup
	readonly databaseTierSecurityGroup: ISecurityGroup
	readonly loadBalancerTierSecurityGroup: ISecurityGroup

	constructor(scope: Construct, id: string, props?: cdk.StackProps) {
		super(scope, id, props);

		this.vpc = new Vpc(this, 'SfoCdkVpc', {
			cidr: '10.0.0.0/20',
			maxAzs: 2,
			subnetConfiguration: [
				{
					cidrMask: 26,
					name: 'PublicSubnet',
					subnetType: SubnetType.PUBLIC,
				},
				{
					cidrMask: 26,
					name: 'PrivateSubnet',
					subnetType: SubnetType.PRIVATE_ISOLATED,
				},
			],
		});

		this.webTierSecurityGroup = new SecurityGroup(this, 'SfoCdkWebSG', {
			vpc: this.vpc,
			securityGroupName: 'sfo-cdk-web-sg',
			allowAllOutbound: false
		});

		this.databaseTierSecurityGroup = new SecurityGroup(this, 'SfoCdkDatabaseSG', {
			vpc: this.vpc,
			securityGroupName: 'sfo-cdk-db-sg',
			allowAllOutbound: false
		});

		this.loadBalancerTierSecurityGroup = new SecurityGroup(this, 'SfoCdkLoadBalancerSG', {
			vpc: this.vpc,
			securityGroupName: 'sfo-cdk-alb-sg',
			allowAllOutbound: false
		});

		this.loadBalancerTierSecurityGroup.addIngressRule(Peer.anyIpv4(), Port.HTTPS);
		this.loadBalancerTierSecurityGroup.addIngressRule(Peer.anyIpv6(), Port.HTTPS);
		this.loadBalancerTierSecurityGroup.addEgressRule(this.webTierSecurityGroup, Port.HTTP);

		this.webTierSecurityGroup.addIngressRule(this.loadBalancerTierSecurityGroup, Port.HTTP);
		this.webTierSecurityGroup.addEgressRule(Peer.anyIpv4(), Port.HTTPS);
		this.webTierSecurityGroup.addEgressRule(this.databaseTierSecurityGroup, Port.MSSQL);

		this.databaseTierSecurityGroup.addIngressRule(this.webTierSecurityGroup, Port.MSSQL);
	}
}
