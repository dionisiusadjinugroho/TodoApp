## Tools :
1. DB Browser(SQLite)
2. Visual Studio 2022
3. Docker Desktop v4.34.3

## Notes :
Limitation current configuration on this Docker - http only, https not working currently
Docker Windows Container need to be change to Linux Container for Bridge Network.
As Basically Docker on 1 Container will isolate the connection. Cause we need to communicate between container.

## Step by step to configure :
1. Open DB Browser(SQLite) - Execute script patching. **Scripts/DATA_PATCHING.sql** for generate 1 User Admin and 1000000 Activities Records for Admin.
![image](https://github.com/user-attachments/assets/716f2fa1-6a18-43b5-8f96-040f1f1f856d)

2. Docker configure for TodoWebAPI
3. Open command prompt and these scripts below - dir to project WebAPI that have dockerignore file. Example :
**cd "....\TodoApp\TodoWebAPI"**
![image](https://github.com/user-attachments/assets/da3f25b5-e70f-430a-b3ee-c1224c38b663)

**docker build -t todowebapi -f TodoWebAPI/Dockerfile .**
![image](https://github.com/user-attachments/assets/77fce95a-2c31-4f51-86fb-d8d8a200e7e3)

**docker run -d --network my_network -p 8080:8080 --name todowebapidocker todowebapi**
![image](https://github.com/user-attachments/assets/aeaef784-0f5b-464b-99f1-4362de214169)

4. Docker configure for TodoBlazorApp
5. Open command prompt and these scripts below - dir to project WebAPI that have dockerignore file. Example :
**cd "....\TodoApp\TodoBlazorApp"**
![image](https://github.com/user-attachments/assets/1211558e-1079-457a-8702-000edfd674fd)

**docker build -t todoblazorapp -f TodoBlazorApp/Dockerfile .**
![image](https://github.com/user-attachments/assets/33a11b5c-3be1-446a-aff9-97b7a5cfa656)

**docker run -d --network my_network -p 4000:4000 --name todoblazorappdocker todoblazorapp**
![image](https://github.com/user-attachments/assets/89cce291-df03-4b43-b472-d5705a0987a4)

## Sample overview webapp running :
![image](https://github.com/user-attachments/assets/c4684576-5b64-4513-a4e3-276358a4bf28)
![image](https://github.com/user-attachments/assets/bc480c30-eba0-47c9-99c9-7f88e6d378e0)
![image](https://github.com/user-attachments/assets/dd6a0a45-0a62-4c53-a36d-832a17a1a055)
![image](https://github.com/user-attachments/assets/1834fbad-1166-45b2-a073-c8d206b2c654)
![image](https://github.com/user-attachments/assets/26445bc6-33f1-4b51-954b-b5cbec60145c)
![image](https://github.com/user-attachments/assets/8886b7b5-662a-4b1e-af3e-8e9727aeffe9)


