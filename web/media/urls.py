from django.urls import path
from . import views

app_name = 'media'

urlpatterns = [
    # path('', views.index, name= 'index')
    path("", views.mainpage, name="main"),
    path("uploadfile/", views.uploadFile, name="uploadFile"),
    path("uploadfile3D/", views.uploadFile_3D, name="uploadFile3D"),
    path("PDF/", views.uploadPDF, name="uploadPDF"),
    path("instruct2D/", views.instruct_2D, name="instruct2D"),
    path("instruct3D/", views.instruct_3D, name="instruct3D"),
    path("script/", views.EnterScript, name="script"),
    path("nickname/", views.putNickname, name="nickname"),
]