
all: 
	dotnet build
	mv bin/Debug/net8.0/ipk24chat-client .

run:
	dotnet run

zip:
	zip -r xseman06.zip Client/ FiniteStateMachine.cs Program.cs LICENSE Makefile README.md Message.cs ipk24chat-client.csproj
	
clean:
	dotnet clean
	