param (
	[Parameter(Mandatory)] [string] $AzureRegistry,
	[Parameter(Mandatory)] [pscredential] $RabbitMQUser
)

if (Test-Path -ErrorAction SilentlyContinue -Path ".env") {
	Rename-Item -NewName "old.env" -Path ".env"
}

Set-Content -Path ".env" -Value "DOCKER_REGISTRY=$AzureRegistry.azurecr.io"

("RabbitMQ.secrets.env", "Server.secrets.env", "Worker.secrets.env") | ForEach-Object {
	if (Test-Path -ErrorAction SilentlyContinue -Path $_) {
		Rename-Item -NewName "old-$_" -Path $_
	}
}

$Password = $RabbitMQUser.GetNetworkCredential().Password
$UserName = $RabbitMQUser.UserName

Set-Content -Path "RabbitMQ.secrets.env" -Value "RABBITMQ_DEFAULT_PASS=$Password`nRABBITMQ_DEFAULT_USER=$UserName"
Set-Content -Path "Server.secrets.env" -Value "RabbitMQ__Pass=$Password`nRabbitMQ__User=$UserName"
Set-Content -Path "Worker.secrets.env" -Value "RabbitMQ__Pass=$Password`nRabbitMQ__User=$UserName"
