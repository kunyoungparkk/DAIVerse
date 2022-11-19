from distutils.command.upload import upload
from email.policy import default
from urllib import request
from django.shortcuts import render, redirect
import hashlib
from firebase import firebase
from django.contrib import messages
import pyrebase
import firebase_admin
from django.core.files.storage import default_storage
from firebase_admin import credentials
from firebase_admin import auth
from google.cloud import storage
from django.utils.datastructures import MultiValueDictKeyError
import pandas as pd

db_url = 'https://daiverse-default-rtdb.firebaseio.com/'
fdb = firebase.FirebaseApplication(db_url, None)

firebaseConfig = {
'apiKey': "AIzaSyCd1ZGGxBrLfsbpqS3QJoiVSMNUmT-dD1E",
'authDomain': "daiverse.firebaseapp.com",
'databaseURL': "https://daiverse-default-rtdb.firebaseio.com",
'projectId': "daiverse",
'storageBucket': "daiverse.appspot.com",
'messagingSenderId': "341731247949",
'appId': "1:341731247949:web:da8bae11a85e83435a5904"
}
firebase = pyrebase.initialize_app(firebaseConfig)
f_storage = firebase.storage()

serviceaccount = {
"type": "service_account",
"project_id": "daiverse",
"private_key_id": "124946d48d392368e3849547fb36dcc39c16c8a7",
"private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDBPtYsz4L5Muao\nQgtDQo6tzjPU2LHm9jndVFjvWYYJ6to502NB0G2lFJpsSrRGqK+4XQqUt0s+fW03\n6eg6ZBN/HruBi5lXfk0CDZpSq/caal6XZzKEVu88bRzxbXgerWae/jLDkwlewmil\n7nRMA11o9aPFq8Va8lstqE8u367WdI7pWQM66+oqWNb7wCTGEiDUj3Hvda/j0iQs\nI7YmDavq7Zuhengop0S+qvdvemW1f4uqnlC/pXN1yjKM+fVsBp/XEgkl8W9oRJd8\niXCuLzUCeW8Q9GwOIpmS0GyR1WKmPHVn/u90DAuaOe/BKV3730JtzAVjCzhi1349\nmN5fAOs7AgMBAAECggEAC1lgmyWLViANPJDuHIvrffRdxjLYo+QzFjc3uQ0n/wdA\n4L70Fd1Z5pL+HwYtAGUPpEx0R9fJ9WztWTrOm1tLIh7jAmLnHDb8D0AF+yGxWEJP\nItx6DhxeXhZY/E7ZVX7A2e1yDjaGG207V7ZGD87JWrFXnQX9CAJppJXh/uk+2Vik\n0uJchV2+gGM6XKlmoXq1xGhpAE908HOtIxg7czOwsWRwyumGdcEimfdYLL7wZfk6\nsqqURpe1roiMI9jDwXHOvwQFaOd5QMHClIPXYTpI2lmyoxhGmqadrnVn2GSEVtBy\njoWaQ2AIAnpULYeWi8nXEyNrnDWdOnuMLJNUGKZqiQKBgQDf46m+ANrlqKAQkS56\nuh9F+5L5j18WCNFuuCqAG72jMqeJoSPyumk0ETC04fOEN791ZQ1sZJmp0q7eF4lZ\n/yIpQwLT0nfU7c/rX0IPV+eIocE9A834ZYEZKz1IpRldeIkO44jMuImh5o8WTMlR\nhH3n2UETHC8dzLDGjG9YPGMRMwKBgQDc9g0v20iRlY47yLkYjgrguKi5L6a9XCbj\n6aMeKX9YaPQ5d4yTUpJl0NeBVFVONzXqd110HVyjdRMfhufWXWaCj/EPP4d3KgSi\nWOl9wvbD/+X9aBoHgWvxKn8m+oyzVRvtjGFLL//4s7nC8x6yE0EHUXEkZ2qvnVDx\nN3zgN7lN2QKBgF9K0RZz2rdhHC+w+211BpaMyzO5GCmSu0E61awNIpvxlWl40oof\nWWO15Vs3yKP/7leTGTGHZ/fmGlOhBO7vLqUNRojNzf8s8RAnTUCmk3+OCWdk1Sfi\nXB5QTxc6Xh9wjDVwFuu7p6jnLwO4zv8JO2WRDKeLmWo/kwYPA900Zp4NAoGAc5xB\n1hJT6+hmXR1uy0w2oNG9ZfdqtJUsu/8Ym8V2ndl8Pz0vzj1b3+3fdAmeAKieiykd\nOo3KVLQC53H8h4qjAhkyNZo11MGlXxRKRsTlvDN+MJJxXA/5XFux38EkMQiM7kOO\n/HoG4ieXQEtYei/czGluW2/IwE39ddhkfCRrzjECgYEAoFVU3Yn0FTGw2t6Z0j9r\nbjM3gvUm0rcdxP0wA+BfUNXKYAq0LpYnhuRLhY8hc84jinCHggHLaT/riSV40v3p\nO5z4n+apwJnRgWZfVIJ9dFjai8tE9Bo5GrgliXWuQoOu3IOp0DkLWR5kZIkFjS+G\nwFqwvm2J/Kun6wsGz+NJMoE=\n-----END PRIVATE KEY-----\n",
"client_email": "firebase-adminsdk-phc3g@daiverse.iam.gserviceaccount.com",
"client_id": "108950405537918073379",
"auth_uri": "https://accounts.google.com/o/oauth2/auth",
"token_uri": "https://oauth2.googleapis.com/token",
"auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
"client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-phc3g%40daiverse.iam.gserviceaccount.com"
}

firebaseConfig_2 = {
'apiKey': "AIzaSyCd1ZGGxBrLfsbpqS3QJoiVSMNUmT-dD1E",
'authDomain': "daiverse.firebaseapp.com",
'databaseURL': "https://daiverse-default-rtdb.firebaseio.com",
'projectId': "daiverse",
'storageBucket': "daiverse.appspot.com",
'messagingSenderId': "341731247949",
'appId': "1:341731247949:web:da8bae11a85e83435a5904",
"serviceAccount": serviceaccount
}

firebase_2 = pyrebase.initialize_app(firebaseConfig_2)
fb_storage = firebase_2.storage()

cred = credentials.Certificate(serviceaccount)
app = firebase_admin.initialize_app(cred, name='auth')
uid = "nm77mC9DMKhV3wC6co6RF9lQkSd2"
auth.get_user(uid, app)
custom_token = auth.create_custom_token(uid, app=app)


def return_context(username):
    username = str(username)
    storage_client = storage.Client()
    dic = {}
    dic_3d = {}
    pdf = ""
    blobs = storage_client.list_blobs("daiverse.appspot.com")

    for blob in blobs:
        if '/' + username + '/2D/' in blob.name:
            a = blob.name.split('/2D/')[1]
            if len(a) != 0:
                dic[int(a.split('/')[0])] = a.split('/')[1]
        
        elif '/' + username + '/3D/' in blob.name:
            b = blob.name.split('/3D/')[1]
            if len(b) != 0:
                dic_3d[int(b.split('/')[0])] = b.split('/')[1]

        elif '/' + username + '/pdf/1/' in blob.name:
            pdf = blob.name.split('/pdf/1/')[1]
                
    titles = ["" for _ in range(10)]
    titles_3d = ["" for _ in range(10)]
    
    for i, j in dic.items():
        titles[i-1] = j
    
    for i, j in dic_3d.items():
        titles_3d[i-1] = j

    type_list = []
    for i in range(10):
        try:
            a = fdb.get('/users/'+ hashlib.sha256(username.encode()).hexdigest() + f'/3D/_{i+1}', None).split('.csv_')[1]
            type_list.append(a)
        except IndexError:
            type_list.append('d')

    nick = fdb.get('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/nickname', None)
    if nick == "" or nick is None:
        nickname = '익명' + username 
    else:
        nickname = nick

    contexts =  {'filetitle_1': titles[0],
            'filetitle_2': titles[1],
            'filetitle_3': titles[2],
            'filetitle_4': titles[3],
            'filetitle_5': titles[4],
            'filetitle_6': titles[5],
            'filetitle_7': titles[6],
            'filetitle_8': titles[7],
            'filetitle_9': titles[8],
            'filetitle_10': titles[9],
            '3_filetitle_1':titles_3d[0],
            '3_filetitle_2':titles_3d[1],
            '3_filetitle_3':titles_3d[2],
            '3_filetitle_4':titles_3d[3],
            '3_filetitle_5':titles_3d[4],
            '3_filetitle_6':titles_3d[5],
            '3_filetitle_7':titles_3d[6],
            '3_filetitle_8':titles_3d[7],
            '3_filetitle_9':titles_3d[8],
            '3_filetitle_10':titles_3d[9],
            'type_1':type_list[0],
            'type_2':type_list[1],
            'type_3':type_list[2],
            'type_4':type_list[3],
            'type_5':type_list[4],
            'type_6':type_list[5],
            'type_7':type_list[6],
            'type_8':type_list[7],
            'type_9':type_list[8],
            'type_10':type_list[9],
            'pdf':pdf,
            'nickname':nickname}

    return {i:j.split('.csv')[0] for i, j in contexts.items()}

def mainpage(request):
    if not request.user.is_authenticated:
        return redirect('accounts:login')
    if fdb.get('/users/'+ hashlib.sha256(str(request.user).encode()).hexdigest() + '/nickname', None) is None or fdb.get('/users/'+ hashlib.sha256(str(request.user).encode()).hexdigest() + '/nickname', None) == "":
        return render(request, 'media/SetNickname.html')
    else:
        return render(request, 'media/main.html', context=return_context(request.user))

def instruct_2D(request):
    if request.user.is_authenticated:
        return render(request, 'media/2Dinstruct.html')
    else:
        return redirect('accounts:login')

def instruct_3D(request):
    if request.user.is_authenticated:
        return render(request, 'media/3Dinstruct.html')
    else:
        return redirect('accounts:login')

def fileupload(blob_lst, uploadFile, num, dim, username):
    username = str(username)
    is_True = ['/' + username + dim + num + "/" in b for b in blob_lst]
    default_storage.save(uploadFile.name, uploadFile)

    if '.xlsx' in uploadFile.name:
        filename = uploadFile.name.split(".xlsx")[0]
        pd.read_excel("savefile/" + uploadFile.name, engine='openpyxl').to_csv("savefile/" + filename + ".csv", encoding='utf-8-sig', index=False)
        default_storage.delete(uploadFile.name)

        if True in is_True:
            del_file_name = blob_lst[is_True.index(True)]    
            fb_storage.delete(del_file_name, custom_token.decode('utf-8'))
            f_storage.child('root/' + username + dim + num + "/" + filename + ".csv").put("savefile/" + filename + ".csv")
            default_storage.delete(filename + ".csv")
        else:
            f_storage.child('root/' + username + dim + num + "/" + filename + ".csv").put("savefile/" + filename + ".csv")
            default_storage.delete(filename + ".csv")

    else:
        if True in is_True:
            del_file_name = blob_lst[is_True.index(True)]
            fb_storage.delete(del_file_name, custom_token.decode('utf-8'))
            f_storage.child('root/' + username + dim + num + "/" + uploadFile.name).put("savefile/" + uploadFile.name)
            default_storage.delete(uploadFile.name)
        else:
            f_storage.child('root/' + username + dim + num + "/" + uploadFile.name).put("savefile/" + uploadFile.name)
            default_storage.delete(uploadFile.name)

def put_graph2d(username):
    username = str(username)
    storage_client = storage.Client()
    blobs = storage_client.list_blobs("daiverse.appspot.com")
    blob_lst = [blob.name for blob in blobs]
    dim = '/2D/'
    for blob in blob_lst:
        if '/' + username + dim in blob:
            blob_num = blob.split(username + dim)[1].split('/')[0]
            fdb.patch('/users/' + hashlib.sha256(username.encode()).hexdigest() + dim, {f'_{blob_num}':blob})

def put_graph3d(barlist, username):
    username = str(username)
    storage_client = storage.Client()
    blobs = storage_client.list_blobs("daiverse.appspot.com")
    blob_lst = [blob.name for blob in blobs]
    dim = '/3D/'
    for blob in blob_lst:
        if '/' + username + dim in blob:
            blob_num = blob.split(username + dim)[1].split('/')[0]
            if int(blob_num) in barlist:
                fdb.patch('/users/' + hashlib.sha256(username.encode()).hexdigest() + dim, {f'_{blob_num}':blob+'_b'})
            else:
                fdb.patch('/users/' + hashlib.sha256(username.encode()).hexdigest() + dim, {f'_{blob_num}':blob+'_d'})

def put_pdf(username):
    username = str(username)
    storage_client = storage.Client()
    blobs = storage_client.list_blobs("daiverse.appspot.com")
    blob_lst = [blob.name for blob in blobs]
    dim = '/pdf/1/'
    for blob in blob_lst:
        if username + dim in blob:
            fdb.patch('/users/' + hashlib.sha256(username.encode()).hexdigest() + '/', {'pdf':blob})

def uploadFile(request):
    if not request.user.is_authenticated:
        return redirect('accounts:login')
    storage_client = storage.Client()
    blobs = storage_client.list_blobs("daiverse.appspot.com")
    blob_lst = [blob.name for blob in blobs]
    if request.method == 'POST':

        try:
            uploadedFile_1 = request.FILES["uploadedFile_1"]
            fileupload(blob_lst, uploadedFile_1, '1' ,"/2D/",request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_2 = request.FILES["uploadedFile_2"]
            fileupload(blob_lst, uploadedFile_2, '2',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_3 = request.FILES["uploadedFile_3"]
            fileupload(blob_lst, uploadedFile_3, '3',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_4 = request.FILES["uploadedFile_4"]
            fileupload(blob_lst, uploadedFile_4, '4',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_5 = request.FILES["uploadedFile_5"]
            fileupload(blob_lst, uploadedFile_5, '5',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_6 = request.FILES["uploadedFile_6"]
            fileupload(blob_lst, uploadedFile_6, '6',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_7 = request.FILES["uploadedFile_7"]
            fileupload(blob_lst, uploadedFile_7, '7',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_8 = request.FILES["uploadedFile_8"]
            fileupload(blob_lst, uploadedFile_8, '8',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_9 = request.FILES["uploadedFile_9"]
            fileupload(blob_lst, uploadedFile_9, '9',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_10 = request.FILES["uploadedFile_10"]
            fileupload(blob_lst, uploadedFile_10, '10',"/2D/", request.user)
        except MultiValueDictKeyError:
            pass
    
        put_graph2d(request.user)

        return render(request, "media/template.html", context= return_context(request.user))
    else:
        return render(request, "media/template.html", context= return_context(request.user))

def uploadFile_3D(request):
    if not request.user.is_authenticated:
        return redirect('accounts:login')
    storage_client = storage.Client()
    blobs = storage_client.list_blobs("daiverse.appspot.com")
    blob_lst = [blob.name for blob in blobs]

    if request.method == 'POST':
        gtype_1 = request.POST.get("gtype_1")
        gtype_2 = request.POST.get("gtype_2")
        gtype_3 = request.POST.get("gtype_3")
        gtype_4 = request.POST.get("gtype_4")
        gtype_5 = request.POST.get("gtype_5")
        gtype_6 = request.POST.get("gtype_6")
        gtype_7 = request.POST.get("gtype_7")
        gtype_8 = request.POST.get("gtype_8")
        gtype_9 = request.POST.get("gtype_9")
        gtype_10 = request.POST.get("gtype_10")
        gtype_list = [gtype_1, gtype_2, gtype_3, gtype_4, gtype_5, gtype_6, gtype_7, gtype_8, gtype_9, gtype_10]
        gtype_index = [i+1 for i, value in enumerate(gtype_list) if value == 'Bar']

        try:
            uploadedFile_1_3D = request.FILES["3_uploadedFile_1"]
            fileupload(blob_lst, uploadedFile_1_3D, '1' ,"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_2_3D = request.FILES["3_uploadedFile_2"]
            fileupload(blob_lst, uploadedFile_2_3D, '2',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_3_3D = request.FILES["3_uploadedFile_3"]
            fileupload(blob_lst, uploadedFile_3_3D, '3',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_4_3D = request.FILES["3_uploadedFile_4"]
            fileupload(blob_lst, uploadedFile_4_3D, '4',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_5_3D = request.FILES["3_uploadedFile_5"]
            fileupload(blob_lst, uploadedFile_5_3D, '5',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_6_3D = request.FILES["3_uploadedFile_6"]
            fileupload(blob_lst, uploadedFile_6_3D, '6',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_7_3D = request.FILES["3_uploadedFile_7"]
            fileupload(blob_lst, uploadedFile_7_3D, '7',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_8_3D = request.FILES["3_uploadedFile_8"]
            fileupload(blob_lst, uploadedFile_8_3D, '8',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_9_3D = request.FILES["3_uploadedFile_9"]
            fileupload(blob_lst, uploadedFile_9_3D, '9',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        try:
            uploadedFile_10_3D = request.FILES["3_uploadedFile_10"]
            fileupload(blob_lst, uploadedFile_10_3D, '10',"/3D/", request.user)
        except MultiValueDictKeyError:
            pass

        put_graph3d(gtype_index, request.user)

        return render(request, "media/3Dupload.html", context= return_context(request.user))
    else:
        return render(request, "media/3Dupload.html", context= return_context(request.user))

def pdfupload(blob_lst, uploadFile, dim, username):
    username = str(username)
    is_True = ['/' + username + dim + '1/' in b for b in blob_lst]

    if True in is_True:
        del_file_name = blob_lst[is_True.index(True)]
        default_storage.save(uploadFile.name, uploadFile)
        fb_storage.delete(del_file_name, custom_token.decode('utf-8'))
        f_storage.child('root/' + username + dim + '1/' + uploadFile.name).put("savefile/" + uploadFile.name)
        default_storage.delete(uploadFile.name)
    else:
        default_storage.save(uploadFile.name, uploadFile)
        f_storage.child('root/' + username + dim + '1/' + uploadFile.name).put("savefile/" + uploadFile.name)
        default_storage.delete(uploadFile.name)

def uploadPDF(request):
    storage_client = storage.Client()
    blobs = storage_client.list_blobs("daiverse.appspot.com")
    blob_lst = [blob.name for blob in blobs]

    if request.method == 'POST':
        try:
            uploadedFile_pdf = request.FILES["uploadedFile_pdf"]
            pdfupload(blob_lst, uploadedFile_pdf, "/pdf/", request.user)
        except MultiValueDictKeyError:
            pass

        put_pdf(request.user)

        return render(request, "media/PDFupload.html", context= return_context(request.user))
    else:
        return render(request, "media/PDFupload.html", context= return_context(request.user))

def putNickname(request):
    username = str(request.user)
    exist_nick = fdb.get('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/nickname', None)
    if request.method == 'POST':
        if not request.user.is_authenticated:
            return redirect('accounts:login')
        nickname = request.POST['nickname']
        string = "'!@#$%^&*()+=\|}{[]:;<>?/\""
        if len(nickname) > 6 or True in list(map(lambda x:x in string, nickname)):
            messages.warning(request, '닉네임은 6글자 이하 및 특수문자를 제거해 주십시오.')
        else:
            fdb.patch('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/', {'nickname' : nickname})
            new_nick = fdb.get('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/nickname', None)
            return render(request, 'media/SetNickname.html', context={'nick': new_nick})
    else:
        return render(request, 'media/SetNickname.html', context={'nick': exist_nick})

    return render(request, 'media/SetNickname.html', context={'nick': ""})

def EnterScript(request):
    username = str(request.user)
    exist_script = fdb.get('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/script', None)
    if request.method == 'POST':
        if not request.user.is_authenticated:
            return redirect('accounts:login')
        script = request.POST['script']
        fdb.patch('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/', {'script' : script})
        new_script = fdb.get('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/script', None)
        return render(request, 'media/main.html', context={'script': new_script})
    else:
        return render(request, 'media/ScriptUpload.html', context={'script': exist_script})

    