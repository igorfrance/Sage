Type.registerNamespace("aeon.controls"),aeon.controls.Tooltip=function(n,t){this.$super(n);var i=aeon.controls.Tooltip;this.settings=new aeon.controls.TooltipSettings(t,n),this.element.bind("mouseenter",Function.createDelegate(this,this.onMouseOver)),this.element.bind("mouseleave",Function.createDelegate(this,this.onMouseOut)),this.element.bind("focus",Function.createDelegate(this,this.onFocus)),this.element.bind("blur",Function.createDelegate(this,this.onBlur)),this.element.bind("click",Function.createDelegate(this,this.onClick)),$(i.document).bind("keydown click",Function.createDelegate(this,this.onDocumentEvent)),this.showTooltip=Function.createDelegate(this,this.show),i.popup.bind("click",Function.createDelegate(this,this.onPopupClick))},aeon.controls.Tooltip.inherits(aeon.controls.HtmlControl),aeon.controls.Tooltip.POPUP_ELEMENT_ID="tooltip",aeon.controls.Tooltip.setup=function(){$.fn.tooltip=aeon.controls.Tooltip.tooltip,aeon.controls.Tooltip.initializeCoordinates(),aeon.controls.Tooltip.createPopupLayer(),$(window).resize(aeon.controls.Tooltip.initializeCoordinates)},aeon.controls.Tooltip.tooltip=function(n){return this.each(function(t,i){aeon.controls.Tooltip.initializeSingleElement(i,n)})},aeon.controls.Tooltip.initializeCoordinates=function(){var t=aeon.controls.Tooltip,u,r,i,f,n;t.windowTop=0,t.windowLeft=0,u=!1;try{t.document=top.document}catch(e){t.document=document,u=!0}if(u)return;r=window;if(window!=window.parent)while(r!=top)for(r=r.parent,i=$("iframe, frame",r.document),f=!1,n=0;n<i.length;n++)if(i[n].contentWindow==window){t.windowLeft+=i.eq(n).offset().left,t.windowTop+=i.eq(n).offset().top,f=!0;break}},aeon.controls.Tooltip.createPopupLayer=function(){var n=aeon.controls.Tooltip,f=$(n.document.getElementById(n.POPUP_ELEMENT_ID)),e,t,u;if(f.length!=0)n.popup=f;else{e='<div id="{0}"><div class="arrow"></div><div class="bg"></div><div class="content"></div></div>'.format(n.POPUP_ELEMENT_ID),n.popup=$(e),$(n.document.body).append(n.popup);if(n.document!=document){t=$("<style id='tooltipStyles'></style>"),$(n.document.body).append(t);var o=t[0],r=[],i=$css.findRules("#tooltip"),s=0;for(u in i)r.push(i[u].cssText);t.html(r.join("\n"))}}n.target=n.popup.find(".content"),n.arrow=n.popup.find(".arrow")},aeon.controls.Tooltip.prototype.hide=function(){var t,n;this.element.attr("title",this.element.attr("_title")),this.element.removeAttr("_title"),t=aeon.controls.Tooltip,t.current=null,n=this.settings.className,this.settings.useFades?t.popup&&t.popup.fadeOut(function(){n&&aeon.controls.Tooltip.popup.removeClass(n)}):(aeon.controls.Tooltip.popup.hide(),n&&aeon.controls.Tooltip.popup.removeClass(n))},aeon.controls.Tooltip.prototype.show=function(){var u,n,h,a;if(this.element.hasClass("tooltip-suspend"))return;if(this.settings.obscureOnly){u=this.element[0];if(u.scrollWidth<=u.offsetWidth&&u.scrollHeight<=u.offsetHeight)return}n=aeon.controls.Tooltip,h=this.element.attr("title")||this.element.attr("data-title")||this.element.attr("_title"),this.element.attr("_title",h),this.element.attr("title",String.EMPTY),a=this.settings.maxWidth||$(n.document).width()*.75,n.popup.css("maxWidth",a),n.target.html(h);var o=this.settings.orientation,s=o[0].match(/T|L|R|B/)?o[0]:"T",l=o[1].match(/T|L|R|B|C/)?o[1]:"L",n=aeon.controls.Tooltip,e=$("html",n.document)[0],f=$("body",n.document)[0],c=e.scrollWidth||f.scrollWidth,v=e.scrollHeight||f.scrollHeight,y=e.scrollTop||f.scrollTop,p=e.scrollLeft||f.scrollLeft;n.popup.css({left:0,top:-1e3,display:"block"}).removeClass("t l r b");if(s.match(/T|B/))var t=this.getPrimaryTop(s,0,v),r=this.getSecondaryLeft(l,0,c),i={popupLeft:r.left,popupTop:t.top,arrowLeft:r.arrowLeft,arrowRight:r.arrowRight,arrowTop:t.arrowTop,arrowBottom:t.arrowBottom,orientation:t.orientation};else var t=this.getPrimaryLeft(s,0,c),r=this.getSecondaryTop(l,0,v),i={popupLeft:t.left,popupTop:seondary.top,arrowLeft:t.arrowLeft,arrowRight:t.arrowRight,arrowTop:r.arrowTop,arrowBottom:r.arrowBottom,orientation:t.orientation};n.popup.css({left:i.popupLeft,top:i.popupTop}),n.arrow.css({left:i.arrowLeft,top:i.arrowTop}),n.popup.addClass(i.orientation.toLowerCase()),n.current=this,this.settings.useFades&&n.popup.fadeIn()},aeon.controls.Tooltip.prototype.getPrimaryLeft=function(n,t,i){var r={orientation:n,left:0,arrowLeft:0,arrowRight:0},o=6,s=aeon.controls.Tooltip,u=s.windowLeft+this.element.offset().left,h=this.element.width(),e=s.popup.outerWidth()+o,f=this.settings.offset;return n=="L"?(r.left=u-e-f,r.arrowLeft="auto",r.arrowRight=0):(r.left=u+h+o+f,r.arrowLeft=0,r.arrowRight="auto"),r.left<t?(r.left=u+h+f,r.orientation="R",r.arrowLeft=0,r.arrowRight="auto"):r.left+e>i&&(r.left=u-e-f,r.orientation="L",r.arrowLeft="auto",r.arrowRight=0),r},aeon.controls.Tooltip.prototype.getSecondaryLeft=function(n,t,i){var r={left:0,arrowLeft:0,arrowRight:0},h=aeon.controls.Tooltip,o=h.windowLeft+this.element.offset().left,s=this.element.width(),u=h.popup.outerWidth(),e=12,f;return n=="L"?(r.left=o,r.arrowLeft=e):n=="R"?(r.left=o+s-u,r.arrowLeft=u-e*2):(r.left=o+(s/2-u/2),r.arrowLeft=u/2-e/2),r.left<t?(r.left=o,r.arrowLeft=e,n="L",r.left+u>i&&(f=r.left+u-i,r.left-=f,r.arrowLeft+=f)):r.left+u>i&&(r.left=o+s-u,r.arrowLeft=u-e*2,n="R",r.left<t&&(f=t-r.left,r.left+=f,r.arrowLeft-=f)),s<e*3&&(f=e*1.5-s/2,n=="L"?r.left-=f:r.left+=f),r},aeon.controls.Tooltip.prototype.getPrimaryTop=function(n,t,i){var r={orientation:n,top:0,arrowTop:0,arrowBottom:0},h=6,c=aeon.controls.Tooltip,f=c.windowTop+this.element.offset().top,l=this.element.height(),o=c.popup.outerHeight()+h,e=this.settings.offset,u,s;return n=="T"?(r.top=f-o-e,r.arrowTop="auto",r.arrowBottom=0):(r.top=f+l+h+e,r.arrowTop=0,r.arrowBottom="auto"),r.top<t?(u=f+l+e,s=r.top-u,r.top=u,r.orientation="B",r.arrowTop=0,r.arrowBottom="auto"):r.top+o>i&&(u=f-o-e,s=r.top-u,r.top=u,r.orientation="T",r.arrowTop="auto",r.arrowBottom=0),r},aeon.controls.Tooltip.prototype.getSecondaryTop=function(n,t,i){var r={top:0,arrowTop:0,arrowBottom:0},f=12,h=aeon.controls.Tooltip,o=h.windowTop+this.element.offset().top,s=this.element.height()+f,u=h.popup.outerHeight(),e;return n=="B"?(r.top=o,r.arrowTop=f):n=="T"?(r.top=o+s-u,r.arrowTop=u-f*2):(r.top=o+(s/2-u/2),r.arrowTop=u/2-f/2),r.top<t?(r.top=o,r.arrowTop=f,n="T",r.top+u>i&&(e=r.top+u-i,r.top-=e,r.arrowTop+=e)):r.top+u>i&&(r.top=o+s-u,r.arrowTop=u-f*2,n="B",r.top<t&&(e=t-r.top,r.top+=e,r.arrowTop-=e)),s<f*2&&(n=="T"?r.top-=f*.8:r.top+=f*.8),r},aeon.controls.Tooltip.prototype.setSettingsValue=function(n,t){t&&(n=="showOn"||n=="hideOn")&&(t=t.split(/(?:\s*,\s*)|(?:\s+)/)),this.settings[n]=t},aeon.controls.Tooltip.prototype.onMouseOver=function(){window.clearTimeout(this.delayId);var t=aeon.controls.Tooltip;if(t.current&&t.current!=this&&!t.current.settings.hideOn.contains("mouseout"))return;if(!this.settings.showOn.contains("mouseover"))return;if(this.element.hasClass("tooltip-suspend"))return;this.delayId=window.setTimeout(this.showTooltip,this.settings.delay)},aeon.controls.Tooltip.prototype.onMouseOut=function(){window.clearTimeout(this.delayId);if(aeon.controls.Tooltip.current!=this)return;if(!this.settings.hideOn.contains("mouseout"))return;this.hide()},aeon.controls.Tooltip.prototype.onClick=function(){var n=aeon.controls.Tooltip;n.popup.is(":visible")&&this.settings.hideOn.contains("click")?this.hide():!n.popup.is(":visible")&&this.settings.showOn.contains("click")&&this.show()},aeon.controls.Tooltip.prototype.onPopupClick=function(){aeon.controls.Tooltip.current==this&&this.settings.hideOn.contains("click")&&this.hide()},aeon.controls.Tooltip.prototype.onDocumentEvent=function(n){var t=aeon.controls.Tooltip;if(!t.popup.is(":visible")||t.current!=this)return;jQuery.contains(this.element[0],n.target)||jQuery.contains(t.popup[0],n.target)||this.hide()},aeon.controls.Tooltip.prototype.onFocus=function(){this.settings.showOn.contains("focus")&&this.show()},aeon.controls.Tooltip.prototype.onBlur=function(){this.settings.hideOn.contains("blur")&&this.hide()},aeon.controls.Tooltip.prototype.toString=function(){return"Tooltip"},aeon.controls.ControlRegistry.registerControl(aeon.controls.Tooltip,".tooltip"),aeon.controls.TooltipSettings=function(n,t){this.$super(n,t),this.className=this.getString("className",n,t,null),this.followMouse=this.getBoolean("followMouse",n,t,!0),this.maxWidth=this.getString("maxWidth",n,t,null),this.showOn=this.getString("showOn",n,t,"mouseover").split(/(?:\s*,\s*)|(?:\s+)/),this.hideOn=this.getString("hideOn",n,t,"mouseout").split(/(?:\s*,\s*)|(?:\s+)/),this.orientation=this.getString("orientation",n,t,"TL").toUpperCase(),this.offset=this.getNumber("offset",n,t,3),this.delay=this.getNumber("delay",n,t,200),this.obscureOnly=this.getBoolean("obscureOnly",n,t,!1),this.useFades=this.getBoolean("useFades",n,t,!1)},aeon.controls.TooltipSettings.inherits(aeon.Settings);
