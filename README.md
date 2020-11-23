# Pre-requisites
- Update the submodules using this command `git submodule update --init`

# Endpoints

## /api/rebuild/file

This endpoint allows a client service to send a unprotected file using Multipart form upload method and returns a protected file in response.

##This endpoint does the following:
- Receives the file as mutlipart bytes array and rebuilds it.
- If it is unsuccessful it returns BAD REQUEST
- If it is successful then it will send the file to the core engine.
- File name is extracted from the URL.
- The core engine will then try and determine the file type
- The file is protected with the default content management flags for the file type.
- Protected file is returned

# Two ways to deploy this
### 1. Without docker
- Install the dotnet runtime on linux using this command:
`sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y aspnetcore-runtime-5.0`
you can find different version of linux and their respective commands here: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#install-the-runtime
- After cloning the repository go inside this path `/source/service` and run below command
`dotnet run`
you will see the project is running and it will show you the base url with port number where api's is running something like this: `https://localhost:5001`

## 2. With docker
- In this path `/source/service` you will see the docker file which you can use to build the image and run the project.
- You can also refer to this url on how to build and run a docker image https://docs.docker.com/engine/examples/dotnetcore/#build-and-run-the-docker-image

## Video demo: 
- https://www.loom.com/share/17df8fa04d634ca69cfa04b7b3d2a96b
