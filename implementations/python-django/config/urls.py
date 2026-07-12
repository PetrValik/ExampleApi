"""Root URL configuration mapping the wire contract to views."""

from django.urls import path

from articles import views

urlpatterns = [
    path("health", views.HealthView.as_view()),
    path("auth/token", views.TokenView.as_view()),
    path("api/articles", views.ArticleListCreateView.as_view()),
    path("api/articles-concurrent", views.ArticleBatchCreateView.as_view()),
    path("api/articles/<int:article_id>", views.ArticleDetailView.as_view()),
]
