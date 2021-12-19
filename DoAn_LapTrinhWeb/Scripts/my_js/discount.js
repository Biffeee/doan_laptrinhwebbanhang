(function(){
  var producto = document.querySelectorAll(".bs-product");
  producto.forEach(function(x){
    x.style.width = "25%";
    x.style.height= "200px";
    x.style.position = "relative";
    x.style.background = "rgb(221,221,221)";
    x.style.margin = " 1rem 0 1rem 1rem";
    x.style.boxSizing = "border-box";
    x.insertAdjacentHTML('afterend', '<pre style="width:calc(75% - 4rem) ; display:block;padding: 1rem; margin:1rem; box-sizing:border-box; background:#eaeaea; border: 0 none" class="prettyprint lang-html"><xmp>'+x.innerHTML.replace(/<span>.+?<\/span>/, `  <span>{{ product.discountRate  | floor }}</span>`)+'</xmp></pre>');
    console.log(x.innerHTML)
  })
})()