'use strict';
 
var gulp = require('gulp');
var sass = require('gulp-sass');
 
sass.compiler = require('node-sass');
//gulp sass:watch
gulp.task("sass:watch",async()=>{
  gulp.watch("./App_Themes/*.scss",async()=>{

    gulp.src('./App_Themes/*.scss')
    .pipe(sass().on('error', sass.logError))
    .pipe(gulp.dest('./App_Themes/'));
  
  })//watch end;

})//task end;