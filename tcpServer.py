from socket import *
import thread
import time

buffer = 256
host = '127.0.0.1'# must be input parameter @TODO
port = 4567 # must be input parameter @TODO
currentWorking = 0
users = [0,0,0]

def response(key):
	return 'Server response: ' + key

def handler(clientsock,addr, user):
	try:
		while 1:
			time.sleep(1.0)
			if user == -1:
				clientsock.send("Error: Server Full")
				break
			data = clientsock.recv(buffer)
			if not data: break
			users[user] = float(data)
			for i in range(len(users)):
				time.sleep(1.0)
				print str(i) , " : " , users[i]
				if user == i:
					clientsock.send(str(i) + ":" + str(users[i]) + ":M") #ME
				else:
					clientsock.send(str(i) + ":" + str(users[i]) + ":Y") #YOU
	except Exception, e:
		print e
		users[user] = 0
		print addr, 'was disconnected.'
	clientsock.close()

if __name__=='__main__':
	ADDR = (host, port)
	serversock = socket(AF_INET, SOCK_STREAM)
	serversock.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
	serversock.bind(ADDR)
	serversock.listen(5)
	print 'OpenBVE Multiplayer Server v0.1 | Codename Lexington |'
	print 'Listening on' ,host, '| Port :', port
	while 1:
		clientsock, addr = serversock.accept()
		print 'Connection from:', addr
		for i in range(len(users)):
			if users[i] == 0:
				currentWorking = i
				break
			else:
				currentWorking = -1
		thread.start_new_thread(handler, (clientsock, addr, currentWorking))