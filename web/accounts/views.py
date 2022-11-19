from django.shortcuts import render
from django.shortcuts import render, redirect
from django.contrib import messages
from django.contrib.auth import login as auth_login, logout as auth_logout, update_session_auth_hash
from django.contrib.auth.forms import AuthenticationForm, PasswordChangeForm
from .forms import CustomUserCreationForm
from django.contrib.auth.decorators import login_required
from django.views.decorators.http import require_http_methods, require_POST
from urllib import request
import hashlib
from firebase import firebase
from django.utils.datastructures import MultiValueDictKeyError
from .models import User
# Create your views here.


db_url = 'https://daiverse-default-rtdb.firebaseio.com/'
fdb = firebase.FirebaseApplication(db_url, None)

@require_http_methods(['GET', 'POST'])
def login(request):
    if request.user.is_authenticated:
        return redirect('media:main')
    
    if request.method == 'POST':
        username = request.POST['username']
        password = request.POST['password']
        form = AuthenticationForm(request, request.POST)
        if hashlib.sha256(password.encode()).hexdigest() == fdb.get('/users/'+ hashlib.sha256(username.encode()).hexdigest() + '/password',None):
            if form.is_valid():
                auth_login(request, form.get_user())
                return redirect('media:main')
            else:
                form = CustomUserCreationForm(request.POST)
                user = User.objects.create_user(username = username, password = password)
                auth_login(request, user)
                return redirect('media:main')
                    
        else:
            messages.warning(request, 'ID 또는 비밀번호를 잘못 입력했습니다.')
    form = AuthenticationForm()
    context = {
        'form': form,
    }
    
    return render(request, 'accounts/login.html', context)


@require_POST
def logout(request):
    if request.user.is_authenticated:
        auth_logout(request)
    return redirect('accounts:login')
