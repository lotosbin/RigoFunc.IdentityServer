FROM microsoft/dotnet:1.0.0-preview1
MAINTAINER lotosbin
RUN     mkdir /code
ADD    ./ /code/
WORKDIR /code/
CMD    dotnet restore && dotnet build ./src/Host && dotnet run ./src/Host